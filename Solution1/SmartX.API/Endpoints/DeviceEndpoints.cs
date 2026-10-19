using SmartX.API.Models;
using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    // What the Command Stream page shows for one device
    public record DeviceView(Guid Id, string Name, string MacAddress, string Category,
                             bool? IsOn, List<string> ActiveAlerts);

    public static class DeviceEndpoints
    {
        // GET /devices?q=pump&category=Control&hasAlerts=true   (every filter is optional)
        public static void MapDeviceEndpoints(this WebApplication app)
        {
            app.MapGet("/devices",
                (string? q, SensorCategory? category, bool? hasAlerts,
                 SensorRegistry registry, AlertTracker alerts, CommandService commands) =>
                {
                    // Build the lookup tables once: sensorId -> its alert names / on-off state
                    var alertsBySensor = alerts.GetAll()
                        .GroupBy(a => a.SensorId)
                        .ToDictionary(g => g.Key, g => g.Select(a => a.Kind.ToString()).ToList());

                    var states = commands.GetState().ToDictionary(kv => kv.Key, kv => kv.Value);

                    IEnumerable<Sensor> sensors = registry.GetAll();

                    // Search: matches part of the name or MAC, ignoring case
                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        var term = q.Trim();
                        sensors = sensors.Where(s =>
                            s.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            s.MacAddress.Contains(term, StringComparison.OrdinalIgnoreCase));
                    }

                    // Filter: operational category
                    if (category is not null)
                    {
                        sensors = sensors.Where(s => s.Category == category);
                    }

                    // Turn each sensor into the view the page needs
                    var devices = sensors.Select(s => new DeviceView(
                        s.Id,
                        s.Name,
                        s.MacAddress,
                        s.Category.ToString(),
                        s.Category == SensorCategory.Control ? states.GetValueOrDefault(s.Id, false) : null,
                        alertsBySensor.GetValueOrDefault(s.Id) ?? new List<string>()));

                    // Filter: only devices with (or without) active alerts
                    if (hasAlerts is not null)
                    {
                        devices = devices.Where(d => (d.ActiveAlerts.Count > 0) == hasAlerts.Value);
                    }

                    return Results.Ok(devices.ToList());
                });
        }
    }
}