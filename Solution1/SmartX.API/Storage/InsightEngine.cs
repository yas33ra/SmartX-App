using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public enum InsightSeverity { Info, Warning }

    public record DeviceInsight(Guid SensorId, string SensorName, string Rule,
                                InsightSeverity Severity, string Message);

    // Looks at each sensor's recent readings and flags devices that look problematic
    public class InsightEngine
    {
        private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(2);
        private const int AnomalyWindow = 20;          // how many earlier readings count as "normal"
        private const int MinPointsForAnomaly = 10;    // need enough history to know what normal is
        private const double ZThreshold = 3.0;         // standard deviations from normal
        private const double NearLimitFraction = 0.8;  // "near" = within 20% of the limit

        private readonly SensorRegistry _registry;
        private readonly SensorHistory _history;

        public InsightEngine(SensorRegistry registry, SensorHistory history)
        {
            _registry = registry;
            _history = history;
        }

        public List<DeviceInsight> Analyse()
        {
            var insights = new List<DeviceInsight>();
            var now = DateTime.UtcNow;

            foreach (var sensor in _registry.GetAll())
            {
                // Control devices only report when commanded, so silence is normal for them
                if (sensor.Category == SensorCategory.Control) continue;

                // The latest readings, plus one extra: the newest is judged against the ones before it
                var recent = _history.GetLatest(sensor.Id, AnomalyWindow + 1);

                if (recent.Count == 0)
                {
                    if (now - sensor.RegisteredAt > StaleAfter)
                    {
                        insights.Add(Make(sensor, "NeverReported", InsightSeverity.Info,
                            "Registered but has not sent any readings yet."));
                    }
                    continue;
                }

                var latest = recent[^1];

                // Rule 1: silent device
                if (now - latest.TimeStamp > StaleAfter)
                {
                    insights.Add(Make(sensor, "Silent", InsightSeverity.Warning,
                        $"No data for {(now - latest.TimeStamp).TotalMinutes:0} min " +
                        $"(last reading {latest.TimeStamp:HH:mm:ss} UTC)."));
                    continue;   // old data tells us nothing about the device right now
                }

                // Rule 2: close to a critical limit
                var nearMessage = NearLimitMessage(sensor.Category, latest.Value);
                if (nearMessage is not null)
                {
                    insights.Add(Make(sensor, "NearLimit", InsightSeverity.Warning, nearMessage));
                }

                // Rule 3: unusual compared with this sensor's own recent history
                if (recent.Count - 1 >= MinPointsForAnomaly)
                {
                    var baseline = recent.Take(recent.Count - 1).Select(p => p.Value).ToList();
                    double mean = baseline.Average();
                    double std = Math.Sqrt(baseline.Average(v => (v - mean) * (v - mean)));

                    // A floor on the spread so a flat sensor doesn't flag a tiny wiggle
                    double spread = Math.Max(std, Math.Max(0.5, Math.Abs(mean) * 0.02));
                    double z = (latest.Value - mean) / spread;

                    if (Math.Abs(z) >= ZThreshold)
                    {
                        insights.Add(Make(sensor, "UnusualReading", InsightSeverity.Warning,
                            $"Latest reading {latest.Value:0.##} is {Math.Abs(z):0.0} standard deviations " +
                            $"{(z > 0 ? "above" : "below")} the recent average of {mean:0.##}."));
                    }
                }
            }

            return insights.OrderByDescending(i => i.Severity).ToList();
        }

        // Within 20% of a limit, but not over it (over it is already a critical alert)
        private static string? NearLimitMessage(SensorCategory category, double value) => category switch
        {
            SensorCategory.Electrical when value < CriticalRules.PowerSpikeWatts
                                        && value >= CriticalRules.PowerSpikeWatts * NearLimitFraction
                => $"{value:0} W is close to the {CriticalRules.PowerSpikeWatts:0} W spike limit.",

            SensorCategory.Climate when value < CriticalRules.ClimateHigh
                                     && value >= CriticalRules.ClimateHigh * NearLimitFraction
                => $"Reading {value:0.#} is close to the high limit of {CriticalRules.ClimateHigh:0}.",

            SensorCategory.Climate when value > CriticalRules.ClimateLow
                                     && value <= CriticalRules.ClimateLow * NearLimitFraction
                => $"Reading {value:0.#} is close to the low limit of {CriticalRules.ClimateLow:0}.",

            _ => null
        };

        private static DeviceInsight Make(Sensor sensor, string rule, InsightSeverity severity, string message) =>
            new(sensor.Id, sensor.Name, rule, severity, message);
    }
}