using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public static class CriticalRules
    {
        public const double PowerSpikeWatts = 5000;
        public const double ClimateHigh = 50;
        public const double ClimateLow = -10;

        // 0 = normal, higher number = more severe
        public static int GetSeverity(IngestionItem item) => item.Category switch
        {
            SensorCategory.Electrical when item.Value >= PowerSpikeWatts => 3,
            SensorCategory.Climate when item.Value >= ClimateHigh || item.Value <= ClimateLow => 2,
            _ => 0
        };
    }
}