namespace SmartX.Client.Services
{
    // One row in the device list (matches the API's DeviceView)
    public class DeviceInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public string Category { get; set; } = "";
        public bool? IsOn { get; set; }                        // null for non-Control devices
        public List<string> ActiveAlerts { get; set; } = new();
    }

    public class AlertInfo
    {
        public Guid SensorId { get; set; }
        public string SensorName { get; set; } = "";
        public string Kind { get; set; } = "";
    }

    public class AlertsResponse
    {
        public List<AlertInfo> Active { get; set; } = new();
        public long DuplicatesSuppressed { get; set; }
    }

    // One entry in the command history
    public class CommandInfo
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = "";
        public DateTime IssuedAt { get; set; }
    }

    // One suggested action from the pattern engine
    public class SuggestionInfo
    {
        public string Kind { get; set; } = "";          // "Command" or "Search"
        public string Text { get; set; } = "";
        public string? SearchTerm { get; set; }
        public bool? On { get; set; }
        public List<Guid>? SensorIds { get; set; }
        public double Confidence { get; set; }
        public string Evidence { get; set; } = "";

        // What the list box shows
        public override string ToString() => $"{Text}   ({Confidence:0}%)";
    }

    // One device-health finding from the insight engine
    public class InsightInfo
    {
        public Guid SensorId { get; set; }
        public string SensorName { get; set; } = "";
        public string Rule { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Message { get; set; } = "";

        public override string ToString() => $"{SensorName}: {Message}";
    }

    // Shapes used only for sending and receiving

    internal record SearchEventRequest(string Term);
    internal record IssueCommandRequest(bool On, List<Guid>? SensorIds);
    internal class UndoResponse { public CommandInfo? Command { get; set; } }
}