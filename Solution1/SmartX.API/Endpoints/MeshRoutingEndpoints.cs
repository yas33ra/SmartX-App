using SmartX.API.Storage;

namespace SmartX.API.Endpoints;

public static class MeshRoutingEndpoints
{
    private static object Describe(MeshGraph mesh, Guid id) =>
        new { id, name = mesh.NameOf(id), macAddress = mesh.MacOf(id) };

    public static IEndpointRouteBuilder MapMeshRoutingEndpoints(this IEndpointRouteBuilder app)
    {
        // Mark a node as down. {node} can be a MAC address or a sensor id
        app.MapPost("/mesh/nodes/{node}/down", (string node, MeshGraph mesh) =>
        {
            if (!mesh.TryResolve(node, out var id))
                return Results.BadRequest(new { errors = new[] { $"Unknown node: '{node}'." } });

            var error = mesh.SetDown(id);
            if (error is not null)
                return Results.BadRequest(new { errors = new[] { error } });

            return Results.Ok(new { node = mesh.NameOf(id), isDown = true });
        });

        // Bring a node back up
        app.MapPost("/mesh/nodes/{node}/up", (string node, MeshGraph mesh) =>
        {
            if (!mesh.TryResolve(node, out var id))
                return Results.BadRequest(new { errors = new[] { $"Unknown node: '{node}'." } });

            mesh.SetUp(id);
            return Results.Ok(new { node = mesh.NameOf(id), isDown = false });
        });

        // Every device connected to a start node (default: the gateway), via BFS
        app.MapGet("/mesh/reachable", (string? from, MeshGraph mesh, MeshRouter router) =>
        {
            if (!mesh.TryResolve(from ?? "gateway", out var start))
                return Results.BadRequest(new { errors = new[] { $"Unknown node: '{from}'." } });

            var result = router.Bfs(start);

            var devices = result.Hops
                .Where(kv => kv.Key != start)
                .OrderBy(kv => kv.Value)
                .Select(kv => new
                {
                    id = kv.Key,
                    name = mesh.NameOf(kv.Key),
                    macAddress = mesh.MacOf(kv.Key),
                    hops = kv.Value
                })
                .ToList();

            return Results.Ok(new
            {
                start = mesh.NameOf(start),
                startIsDown = mesh.IsDown(start),
                count = devices.Count,
                devices
            });
        });

        // Fewest-hop route from one node to the gateway
        app.MapGet("/mesh/route/{node}", (string node, MeshGraph mesh, MeshRouter router) =>
        {
            if (!mesh.TryResolve(node, out var id))
                return Results.BadRequest(new { errors = new[] { $"Unknown node: '{node}'." } });

            var route = router.RouteToGateway(id);

            if (route is null)
            {
                return Results.Ok(new
                {
                    node = mesh.NameOf(id),
                    reachable = false,
                    isDown = mesh.IsDown(id),
                    message = mesh.IsDown(id)
                        ? "This node is down."
                        : "No working route to the gateway."
                });
            }

            return Results.Ok(new
            {
                node = mesh.NameOf(id),
                reachable = true,
                hops = route.Count - 1,
                route = route.Select(r => Describe(mesh, r))
            });
        });

        // Overall health: connected, down, cut off, and where neighbours of down nodes hop instead
        app.MapGet("/mesh/status", (MeshGraph mesh, MeshRouter router) =>
        {
            var bfs = router.Bfs(MeshGraph.GatewayId);
            var sensors = mesh.GetAllNodeIds().Where(id => id != MeshGraph.GatewayId).ToList();

            var down = sensors.Where(mesh.IsDown).Select(id => Describe(mesh, id)).ToList();

            var connected = sensors
                .Where(id => bfs.Hops.ContainsKey(id))
                .OrderBy(id => bfs.Hops[id])
                .Select(id => new
                {
                    id,
                    name = mesh.NameOf(id),
                    hops = bfs.Hops[id],
                    nextHop = mesh.NameOf(bfs.Parent[id])
                })
                .ToList();

            // Working nodes that BFS could not reach: their path went through a down node
            var cutOff = sensors
                .Where(id => !mesh.IsDown(id) && !bfs.Hops.ContainsKey(id))
                .Select(id => Describe(mesh, id))
                .ToList();

            // For each down node: its working neighbours and where they hop now
            var healing = sensors
                .Where(mesh.IsDown)
                .Select(downId => new
                {
                    downNode = mesh.NameOf(downId),
                    neighbours = mesh.GetNeighbours(downId)
                        .Where(n => !mesh.IsDown(n.To))
                        .Select(n => new
                        {
                            name = mesh.NameOf(n.To),
                            reachable = bfs.Hops.ContainsKey(n.To),
                            nextHop = bfs.Parent.TryGetValue(n.To, out var p) ? mesh.NameOf(p) : null
                        })
                        .ToList()
                })
                .ToList();

            return Results.Ok(new { connected, down, cutOff, healing });
        });

        // The minimum spanning tree: the cheapest set of links connecting everything
        app.MapGet("/mesh/mst", (MeshGraph mesh, MeshRouter router) =>
        {
            var mst = router.MinimumSpanningTree();
            var inTree = mst.CostToGateway.Keys.ToHashSet();

            var edges = mst.Edges.Select(e => new
            {
                from = mesh.NameOf(e.FromId),
                to = mesh.NameOf(e.ToId),
                cost = e.Cost
            });

            // Nodes the tree could not include (down, or cut off by a down node)
            var notInTree = mesh.GetAllNodeIds()
                .Where(id => !inTree.Contains(id))
                .Select(id => new { name = mesh.NameOf(id), isDown = mesh.IsDown(id) });

            return Results.Ok(new
            {
                totalCost = mst.TotalCost,
                edgeCount = mst.Edges.Count,
                edges,
                notInTree
            });
        });

        // Compare the cheapest (MST) route with the fewest-hops (BFS) route for one node
        app.MapGet("/mesh/mst/route/{node}", (string node, MeshGraph mesh, MeshRouter router) =>
        {
            if (!mesh.TryResolve(node, out var id))
                return Results.BadRequest(new { errors = new[] { $"Unknown node: '{node}'." } });

            var mstRoute = router.RouteViaMst(id);

            if (mstRoute is null)
            {
                return Results.Ok(new
                {
                    node = mesh.NameOf(id),
                    reachable = false,
                    message = mesh.IsDown(id)
                        ? "This node is down."
                        : "No working route to the gateway."
                });
            }

            var hopRoute = router.RouteToGateway(id)!;

            string Path(List<Guid> route) =>
                string.Join(" → ", route.Select(mesh.NameOf));

            return Results.Ok(new
            {
                node = mesh.NameOf(id),
                reachable = true,
                powerEfficientRoute = new
                {
                    path = Path(mstRoute.Route),
                    hops = mstRoute.Route.Count - 1,
                    cost = mstRoute.Cost
                },
                fewestHopsRoute = new
                {
                    path = Path(hopRoute),
                    hops = hopRoute.Count - 1,
                    cost = router.RouteCost(hopRoute)
                }
            });
        });
        return app;
    }
}