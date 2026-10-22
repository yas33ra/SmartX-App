namespace SmartX.API.Storage
{
    public record HistoryPoint(DateTime TimeStamp, double Value);

    // One AVL tree per sensor, keyed by timestamp.
    // The tree keeps points sorted and balanced, so time-range queries
    // do not have to scan every reading.
    public class SensorHistory
    {
        private const int MaxPointsPerSensor = 20000;

        private readonly Dictionary<Guid, AvlTree<DateTime, double>> _timelines = new();
        private readonly object _lock = new();

        public void Record(Guid sensorId, DateTime timeStamp, double value)
        {
            lock (_lock)
            {
                if (!_timelines.TryGetValue(sensorId, out var timeline))
                {
                    timeline = new AvlTree<DateTime, double>();
                    _timelines[sensorId] = timeline;
                }

                // Keys must be unique, so nudge by a tick until it is free
                while (timeline.ContainsKey(timeStamp))
                {
                    timeStamp = timeStamp.AddTicks(1);
                }

                timeline.Insert(timeStamp, value);

                // Drop the oldest points if the timeline gets too long
                while (timeline.Count > MaxPointsPerSensor)
                {
                    timeline.TryRemoveMin(out _, out _);
                }
            }
        }

        // Points in time order, optionally filtered to a window and/or the latest N.
        // Empty list if the sensor has no readings yet.
        public List<HistoryPoint> GetTimeline(Guid sensorId, DateTime? since, DateTime? until, int? limit)
        {
            lock (_lock)
            {
                if (!_timelines.TryGetValue(sensorId, out var timeline))
                {
                    return new List<HistoryPoint>();
                }

                List<KeyValuePair<DateTime, double>> entries;

                if (since is null && until is null && limit is > 0)
                {
                    // Fast path: just the newest N, no range needed
                    entries = timeline.Last(limit.Value);
                }
                else
                {
                    // Range query on the tree: skips branches outside the window
                    entries = timeline.Range(
                        since ?? DateTime.MinValue,
                        until ?? DateTime.MaxValue);

                    if (limit is > 0 && entries.Count > limit.Value)
                    {
                        entries = entries.GetRange(entries.Count - limit.Value, limit.Value);
                    }
                }

                return entries
                    .Select(p => new HistoryPoint(p.Key, p.Value))
                    .ToList();
            }
        }

        // The most recent 'count' points, oldest first. Empty if no readings yet.
        public List<HistoryPoint> GetLatest(Guid sensorId, int count)
        {
            lock (_lock)
            {
                if (!_timelines.TryGetValue(sensorId, out var timeline))
                {
                    return new List<HistoryPoint>();
                }

                return timeline.Last(count)
                               .Select(p => new HistoryPoint(p.Key, p.Value))
                               .ToList();
            }
        }

        // How big each tree is and how tall, to show it stays balanced
        public (int Count, int Height) GetTreeStats(Guid sensorId)
        {
            lock (_lock)
            {
                if (!_timelines.TryGetValue(sensorId, out var timeline))
                {
                    return (0, 0);
                }

                return (timeline.Count, timeline.Height);
            }
        }
    }
}