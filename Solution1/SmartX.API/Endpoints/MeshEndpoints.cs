using SmartX.API.Storage;

namespace SmartX.API.Endpoints;

public record AddLinkRequest(string? From, string? To, double Cost);

public static class MeshEndpoints
{
    public static IEndpointRouteBuilder MapMeshEndpoints(this IEndpointRouteBuilder app)
    {
        // The whole mesh: every node (even unlinked ones) and every link
        app.MapGet("/mesh", (MeshGraph mesh, SensorRegistry registry) =>
        {
            string NameOf(Guid id) =>
                id == MeshGraph.GatewayId ? "Gateway" : registry.GetById(id)?.Name ?? id.ToString();

            var nodes = new List<object>
            {
                new
                {
                    id = MeshGraph.GatewayId,
                    name = "Gateway",
                    macAddress = (string?)null,
                    category = "Gateway",
                    isGateway = true,
                    isDown = false,
                    linkCount = mesh.GetNeighbours(MeshGraph.GatewayId).Count
                }
            };

            nodes.AddRange(registry.GetAll().Select(s => (object)new
            {
                id = s.Id,
                name = s.Name,
                macAddress = s.MacAddress,
                category = s.Category.ToString(),
                isGateway = false,
                isDown = mesh.IsDown(s.Id),
                linkCount = mesh.GetNeighbours(s.Id).Count
            }));

            var links = mesh.GetLinks().Select(l => new
            {
                fromId = l.FromId,
                fromName = NameOf(l.FromId),
                toId = l.ToId,
                toName = NameOf(l.ToId),
                cost = l.Cost
            });

            return Results.Ok(new { nodes, links });
        });

        // Add a link. "from" and "to" can be a MAC address, a sensor id, or "gateway"
        app.MapPost("/mesh/links", (AddLinkRequest request, MeshGraph mesh) =>
        {
            var errors = new List<string>();

            if (!mesh.TryResolve(request.From, out var fromId))
                errors.Add($"Unknown 'from' node: '{request.From}'.");
            if (!mesh.TryResolve(request.To, out var toId))
                errors.Add($"Unknown 'to' node: '{request.To}'.");

            if (errors.Count > 0)
                return Results.BadRequest(new { errors });

            var error = mesh.AddLink(fromId, toId, request.Cost);
            if (error is not null)
                return Results.BadRequest(new { errors = new[] { error } });

            return Results.Created("/mesh", new { fromId, toId, cost = request.Cost });
        });

        // Remove a link: DELETE /mesh/links?from=...&to=...
        app.MapDelete("/mesh/links", (string from, string to, MeshGraph mesh) =>
        {
            if (!mesh.TryResolve(from, out var fromId) || !mesh.TryResolve(to, out var toId))
                return Results.BadRequest(new { errors = new[] { "Unknown node." } });

            return mesh.RemoveLink(fromId, toId)
                ? Results.Ok(new { removed = true })
                : Results.NotFound(new { errors = new[] { "No such link." } });
        });

        return app;
    }
}