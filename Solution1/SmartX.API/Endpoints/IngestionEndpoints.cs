using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    public static class IngestionEndpoints
    {
        public static void MapIngestionEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/ingestion");

            group.MapGet("/status", (IngestionPipeline p) =>
            {
                var (standard, critical, processed) = p.GetStatus();
                return Results.Ok(new { standardQueued = standard, criticalQueued = critical, processed, paused = p.Paused });
            });

            group.MapPost("/pause", (IngestionPipeline p) => { p.Paused = true; return Results.Ok(new { paused = true }); });
            group.MapPost("/resume", (IngestionPipeline p) => { p.Paused = false; return Results.Ok(new { paused = false }); });
        }
    }
}