using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    public static class HistoryStatsEndpoints
    {
        public static IEndpointRouteBuilder MapHistoryStatsEndpoints(this IEndpointRouteBuilder app)
        {
            // Shows how many readings are stored and how tall the AVL tree is.
            // A balanced tree's height stays close to log2(count).
            app.MapGet("/history/{sensorId:guid}/stats", (Guid sensorId, SensorHistory history) =>
            {
                var (count, height) = history.GetTreeStats(sensorId);

                int idealHeight = count == 0 ? 0 : (int)Math.Ceiling(Math.Log2(count + 1));

                return Results.Ok(new
                {
                    count,
                    treeHeight = height,
                    idealHeight,
                    note = "An unbalanced tree built from time-ordered data would have height = count."
                });
            });

            return app;
        }
    }
}