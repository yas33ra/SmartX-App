using SmartX.API.Models;

namespace SmartX.API.Storage
{
    // Holds the currently active alerts. HashSet guarantees each (sensor, kind) appears once.
    public class AlertTracker
    {
        private readonly HashSet<AlertKey> _active = new();
        private readonly object _lock = new();
        private long _duplicatesSuppressed;

        // Returns true if this is a NEW alert, false if it was already active
        public bool Raise(Guid sensorId, AlertKind kind)
        {
            lock (_lock)
            {
                bool isNew = _active.Add(new AlertKey(sensorId, kind));
                if (!isNew)
                {
                    _duplicatesSuppressed++;
                }
                return isNew;
            }
        }

        // Clears an alert once the problem is fixed. Returns false if it wasn't active.
        public bool Resolve(Guid sensorId, AlertKind kind)
        {
            lock (_lock) { return _active.Remove(new AlertKey(sensorId, kind)); }
        }

        public List<AlertKey> GetAll()
        {
            lock (_lock) { return _active.ToList(); }
        }

        public long DuplicatesSuppressed
        {
            get { lock (_lock) { return _duplicatesSuppressed; } }
        }
    }
}