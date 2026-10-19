using SmartX.API.Models;

namespace SmartX.API.Storage
{
    // One device's state change inside a command
    public record DeviceChange(Guid SensorId, bool Previous, bool Current);

    // One operator command; may touch many devices
    public record OverrideCommand(Guid Id, string Description, DateTime IssuedAt, List<DeviceChange> Changes);

    public class CommandService
    {
        private readonly Stack<OverrideCommand> _undoStack = new();     // LIFO: last command is undone first
        private readonly Dictionary<Guid, bool> _state = new();         // current on/off per device
        private readonly object _lock = new();
        private readonly IngestionPipeline _pipeline;

        private readonly PatternEngine _patterns;

        public CommandService(IngestionPipeline pipeline, PatternEngine patterns)
        {
            _pipeline = pipeline;
            _patterns = patterns;
        }

        public OverrideCommand Issue(bool on, List<Guid> sensorIds, string description)
        {
            lock (_lock)
            {
                var changes = new List<DeviceChange>();
                foreach (var id in sensorIds)
                {
                    bool previous = _state.GetValueOrDefault(id, false);   // devices start OFF
                    changes.Add(new DeviceChange(id, previous, on));
                    _state[id] = on;
                }

                var command = new OverrideCommand(Guid.NewGuid(), description, DateTime.UtcNow, changes);
                _undoStack.Push(command);

                var key = $"cmd:{(on ? "ON" : "OFF")}:{string.Join(",", sensorIds.OrderBy(id => id))}";
                _patterns.RecordAction(new ActionEvent(key, ActionType.Command, description, On: on, SensorIds: sensorIds.ToList()));

                foreach (var change in changes)
                {
                    Publish(change.SensorId, change.Current);
                }
                return command;
            }
        }

        // Reverts the most recent command. Returns null if there is nothing to undo.
        public OverrideCommand? Undo()
        {
            lock (_lock)
            {
                if (!_undoStack.TryPop(out var command))
                {
                    return null;
                }

                foreach (var change in command.Changes)
                {
                    _state[change.SensorId] = change.Previous;
                    Publish(change.SensorId, change.Previous);
                }
                return command;
            }
        }

        public List<OverrideCommand> GetHistory()
        {
            lock (_lock) { return _undoStack.ToList(); }   // enumerates top of stack first
        }

        public List<KeyValuePair<Guid, bool>> GetState()
        {
            lock (_lock) { return _state.ToList(); }
        }

        // Send the new state through the normal ingestion pipeline as a Control reading
        private void Publish(Guid sensorId, bool isOn) =>
            _pipeline.Enqueue(new IngestionItem(sensorId, SensorCategory.Control, isOn ? 1 : 0, DateTime.UtcNow));
    }
}