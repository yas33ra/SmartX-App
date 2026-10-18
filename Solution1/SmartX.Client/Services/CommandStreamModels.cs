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

    // Shapes used only for sending and receiving
    internal record IssueCommandRequest(bool On, List<Guid>? SensorIds);
    internal class UndoResponse { public CommandInfo? Command { get; set; } }
}