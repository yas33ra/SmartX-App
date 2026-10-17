namespace SmartX.API.Storage
{
    public record HistoryPoint(DateTime TimeStamp, double Value);

    // One sorted timeline per sensor, so a chart can plot points without sorting them
    public class SensorHistory
    {
        private const int MaxPointsPerSensor = 20000;

        private readonly Dictionary<Guid, SortedDictionary<DateTime, double>> _timelines = new();
        private readonly object _lock = new();

        public void Record(Guid sensorId, DateTime timeStamp, double value)
        {
            lock (_lock)
            {
                if (!_timelines.TryGetValue(sensorId, out var timeline))
                {
                    timeline = new SortedDictionary<DateTime, double>();
                    _timelines[sensorId] = timeline;
                }

                // SortedDictionary throws on a duplicate key, so nudge by a tick until it is free
                while (timeline.ContainsKey(timeStamp))
                {
                    timeStamp = timeStamp.AddTicks(1);
                }

                timeline[timeStamp] = value;

                // Drop the oldest points if the timeline gets too long
                while (timeline.Count > MaxPointsPerSensor)
                {
                    timeline.Remove(timeline.First().Key);
                }
            }
        }

        // Returns points in time order, optionally filtered to a window and/or the latest N.
        // Returns an empty list if the sensor has no readings yet.
        public List<HistoryPoint> GetTimeline(Guid sensorId, DateTime? since, DateTime? until, int? limit)
        {
            lock (_lock)
            {
                if (!_timelines.TryGetValue(sensorId, out var timeline))
                {
                    return new List<HistoryPoint>();
                }

                IEnumerable<KeyValuePair<DateTime, double>> query = timeline;   // already in order

                if (since is not null) query = query.Where(p => p.Key >= since.Value);
                if (until is not null) query = query.Where(p => p.Key <= until.Value);

                var points = query.Select(p => new HistoryPoint(p.Key, p.Value));

                if (limit is > 0) points = points.TakeLast(limit.Value);

                return points.ToList();
            }
        }
    }
}