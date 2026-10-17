using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    public static class HistoryEndpoints
    {
        // GET /history/{sensorId}?since=2026-10-17T10:00:00Z&until=...&limit=100
        public static void MapHistoryEndpoints(this WebApplication app)
        {
            app.MapGet("/history/{sensorId:guid}",
                (Guid sensorId, DateTime? since, DateTime? until, int? limit,
                 SensorRegistry registry, SensorHistory history) =>
                {
                    var sensor = registry.GetById(sensorId);
                    if (sensor is null)
                    {
                        return Results.NotFound(new { errors = new[] { "Sensor not found." } });
                    }

                    var points = history.GetTimeline(sensorId, since?.ToUniversalTime(), until?.ToUniversalTime(), limit);

                    return Results.Ok(new
                    {
                        sensorId,
                        sensorName = sensor.Name,
                        category = sensor.Category.ToString(),
                        count = points.Count,
                        points
                    });
                });
        }
    }
}