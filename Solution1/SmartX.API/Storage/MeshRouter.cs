namespace SmartX.API.Storage;

// Result of a BFS: how many hops each reachable node is from the start,
// and which node discovered it (its parent, i.e. the next hop back to the start).
public record BfsResult(Dictionary<Guid, int> Hops, Dictionary<Guid, Guid> Parent);

// Finds routes through the mesh. BFS now, the minimum spanning tree comes next.
public class MeshRouter
{
    private readonly MeshGraph _graph;

    public MeshRouter(MeshGraph graph)
    {
        _graph = graph;
    }

    // Breadth-First Search. Nodes that are down are skipped, so they
    // can neither be reached nor passed through.
    public BfsResult Bfs(Guid start)
    {
        var hops = new Dictionary<Guid, int>();
        var parent = new Dictionary<Guid, Guid>();

        if (_graph.IsDown(start)) return new BfsResult(hops, parent);

        var queue = new Queue<Guid>();
        hops[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var (next, _) in _graph.GetNeighbours(current))
            {
                if (hops.ContainsKey(next)) continue;   // already visited
                if (_graph.IsDown(next)) continue;      // down nodes are skipped

                hops[next] = hops[current] + 1;
                parent[next] = current;                 // remember who found it
                queue.Enqueue(next);
            }
        }

        return new BfsResult(hops, parent);
    }

    // Fewest-hop route from a node to the gateway, or null if it is cut off
    public List<Guid>? RouteToGateway(Guid node)
    {
        var result = Bfs(MeshGraph.GatewayId);

        if (!result.Hops.ContainsKey(node)) return null;

        var route = new List<Guid> { node };
        var current = node;

        // Follow the parents back to the gateway
        while (current != MeshGraph.GatewayId)
        {
            current = result.Parent[current];
            route.Add(current);
        }

        return route;
    }
}