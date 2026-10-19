using SmartX.API.Models;
using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    // SensorIds empty or missing = every registered Control device
    public record IssueCommandRequest(bool On, List<Guid>? SensorIds);

    public static class CommandEndpoints
    {
        public static void MapCommandEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/commands");

            group.MapPost("/", (IssueCommandRequest request, SensorRegistry registry, CommandService commands) =>
            {
                bool all = request.SensorIds is null || request.SensorIds.Count == 0;
                var targets = new List<Sensor>();

                if (all)
                {
                    targets = registry.GetAll().Where(s => s.Category == SensorCategory.Control).ToList();
                }
                else
                {
                    var errors = new List<string>();
                    foreach (var id in request.SensorIds!.Distinct())
                    {
                        var sensor = registry.GetById(id);
                        if (sensor is null)
                            errors.Add($"Sensor {id} not found.");
                        else if (sensor.Category != SensorCategory.Control)
                            errors.Add($"'{sensor.Name}' is a {sensor.Category} sensor, not a Control device.");
                        else
                            targets.Add(sensor);
                    }
                    if (errors.Count > 0)
                    {
                        return Results.BadRequest(new { errors });
                    }
                }

                if (targets.Count == 0)
                {
                    return Results.BadRequest(new { errors = new[] { "No control devices are registered." } });
                }

                string state = request.On ? "ON" : "OFF";
                string what = all ? $"all {targets.Count} control devices"
                            : targets.Count == 1 ? targets[0].Name
                            : $"{targets.Count} devices";

                var command = commands.Issue(request.On, targets.Select(t => t.Id).ToList(), $"Turn {what} {state}");
                return Results.Created("/commands/history", command);
            });

            group.MapPost("/undo", (CommandService commands) =>
            {
                var undone = commands.Undo();
                return undone is null
                    ? Results.BadRequest(new { errors = new[] { "Nothing to undo." } })
                    : Results.Ok(new { undone = true, command = undone });
            });

            group.MapGet("/history", (CommandService commands) => Results.Ok(commands.GetHistory()));

            group.MapGet("/state", (CommandService commands, SensorRegistry registry) =>
                Results.Ok(commands.GetState().Select(kv => new
                {
                    sensorId = kv.Key,
                    sensorName = registry.GetById(kv.Key)?.Name ?? "Unknown",
                    isOn = kv.Value
                })));
        }
    }
}