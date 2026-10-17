using SmartX.API.Models;
using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    public record DisconnectRequest(Guid SensorId);

    public static class AlertEndpoints
    {
        public static void MapAlertEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/alerts");

            // List active alerts, with the sensor's name for display
            group.MapGet("/", (AlertTracker tracker, SensorRegistry registry) =>
            {
                var alerts = tracker.GetAll().Select(a => new
                {
                    sensorId = a.SensorId,
                    sensorName = registry.GetById(a.SensorId)?.Name ?? "Unknown",
                    kind = a.Kind.ToString()
                });
                return Results.Ok(new { active = alerts, duplicatesSuppressed = tracker.DuplicatesSuppressed });
            });

            // A device reports it has gone offline
            group.MapPost("/disconnect", (DisconnectRequest request, AlertTracker tracker, SensorRegistry registry) =>
            {
                if (registry.GetById(request.SensorId) is null)
                {
                    return Results.NotFound(new { errors = new[] { "Sensor not found." } });
                }

                bool isNew = tracker.Raise(request.SensorId, AlertKind.Disconnected);
                return isNew
                    ? Results.Created("/alerts", new { isNew = true })
                    : Results.Ok(new { isNew = false, message = "Already disconnected; duplicate alert ignored." });
            });

            // An operator or the system clears an alert
            group.MapDelete("/{sensorId:guid}/{kind}", (Guid sensorId, AlertKind kind, AlertTracker tracker) =>
                tracker.Resolve(sensorId, kind) ? Results.NoContent() : Results.NotFound());
        }
    }
}