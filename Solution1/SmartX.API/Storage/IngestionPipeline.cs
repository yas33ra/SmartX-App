using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public class IngestionPipeline
    {
        private readonly Queue<IngestionItem> _standard = new();                       // FIFO
        private readonly PriorityQueue<IngestionItem, (int, long)> _critical = new();  // most severe first
        private readonly object _lock = new();
        private readonly TelemetryStore _store;
        private long _sequence;     // tie-breaker so equal severity stays first-come-first-served
        private long _processed;

        // When true the worker stops draining (used to demo the queues filling up)
        public volatile bool Paused;

        public IngestionPipeline(TelemetryStore store) => _store = store;

        // Returns true if the item was critical
        public bool Enqueue(IngestionItem item)
        {
            int severity = CriticalRules.GetSeverity(item);
            lock (_lock)
            {
                if (severity > 0)
                {
                    // PriorityQueue pops the SMALLEST priority, so severity is negated
                    _critical.Enqueue(item, (-severity, _sequence++));
                    return true;
                }
                _standard.Enqueue(item);
                return false;
            }
        }

        // Takes the next item (critical first), stores it, and returns it. Null if both queues are empty.
        public IngestionItem? ProcessNext()
        {
            IngestionItem? item;
            lock (_lock)
            {
                if (_critical.TryDequeue(out var c, out _)) item = c;
                else if (_standard.TryDequeue(out var s)) item = s;
                else return null;
            }

            Store(item);
            Interlocked.Increment(ref _processed);
            return item;
        }

        public (int standard, int critical, long processed) GetStatus()
        {
            lock (_lock) { return (_standard.Count, _critical.Count, Interlocked.Read(ref _processed)); }
        }

        // Convert the generic item back into the right typed packet
        private void Store(IngestionItem item)
        {
            switch (item.Category)
            {
                case SensorCategory.Climate:
                    _store.AddClimate(new TelemetryPacket<double>
                    { SensorId = item.SensorId, TimeStamp = item.TimeStamp, Value = item.Value });
                    break;
                case SensorCategory.Electrical:
                    _store.AddElectrical(new TelemetryPacket<PowerReading>
                    { SensorId = item.SensorId, TimeStamp = item.TimeStamp, Value = new PowerReading { Watts = (int)item.Value } });
                    break;
                case SensorCategory.Control:
                    _store.AddControl(new TelemetryPacket<bool>
                    { SensorId = item.SensorId, TimeStamp = item.TimeStamp, Value = item.Value != 0 });
                    break;
            }
        }
    }
}