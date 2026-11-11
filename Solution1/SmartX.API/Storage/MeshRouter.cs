namespace SmartX.API.Storage;

// Result of a BFS: how many hops each reachable node is from the start,
// and which node discovered it (its parent, i.e. the next hop back to the start).
public record BfsResult(Dictionary<Guid, int> Hops, Dictionary<Guid, Guid> Parent);

// Finds routes through the mesh. BFS now, the minimum spanning tree comes next.

public record MstEdge(Guid FromId, Guid ToId, double Cost);

// Edges chosen for the tree, the total cost, each node's parent (next hop
// toward the gateway) and each node's total cost back to the gateway along the tree
public record MstResult(
    List<MstEdge> Edges,
    double TotalCost,
    Dictionary<Guid, Guid> Parent,
    Dictionary<Guid, double> CostToGateway);

public record MstRoute(List<Guid> Route, double Cost);
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

    // Prim's algorithm, growing the tree outward from the gateway.
    // A priority queue (binary min-heap) always gives the cheapest link next.
    // Nodes that are down are left out.
    public MstResult MinimumSpanningTree()
    {
        var edges = new List<MstEdge>();
        var parent = new Dictionary<Guid, Guid>();
        var costToGateway = new Dictionary<Guid, double> { [MeshGraph.GatewayId] = 0 };
        var inTree = new HashSet<Guid> { MeshGraph.GatewayId };

        // Priority = (cost, sequence). The sequence number breaks ties by insertion order.
        var heap = new PriorityQueue<(Guid From, Guid To, double Cost), (double Cost, long Seq)>();
        long sequence = 0;
        double total = 0;

        void AddLinksFrom(Guid node)
        {
            foreach (var (next, cost) in _graph.GetNeighbours(node))
            {
                if (inTree.Contains(next)) continue;
                if (_graph.IsDown(next)) continue;
                heap.Enqueue((node, next, cost), (cost, sequence++));
            }
        }

        AddLinksFrom(MeshGraph.GatewayId);

        while (heap.Count > 0)
        {
            var (from, to, cost) = heap.Dequeue();   // cheapest link out of the tree

            if (inTree.Contains(to)) continue;       // already connected another way

            inTree.Add(to);
            edges.Add(new MstEdge(from, to, cost));
            parent[to] = from;
            costToGateway[to] = costToGateway[from] + cost;
            total += cost;

            AddLinksFrom(to);                        // new links to consider
        }

        return new MstResult(edges, total, parent, costToGateway);
    }

    // Route from a node to the gateway along the MST, with its total cost.
    // Null if the node is not in the tree (down or cut off).
    public MstRoute? RouteViaMst(Guid node)
    {
        var mst = MinimumSpanningTree();

        if (!mst.CostToGateway.ContainsKey(node)) return null;

        var route = new List<Guid> { node };
        var current = node;

        while (current != MeshGraph.GatewayId)
        {
            current = mst.Parent[current];
            route.Add(current);
        }

        return new MstRoute(route, mst.CostToGateway[node]);
    }

    // Adds up the link costs along a route
    public double RouteCost(List<Guid> route)
    {
        double cost = 0;

        for (int i = 0; i < route.Count - 1; i++)
        {
            var link = _graph.GetNeighbours(route[i]).First(n => n.To == route[i + 1]);
            cost += link.Cost;
        }

        return cost;
    }
}