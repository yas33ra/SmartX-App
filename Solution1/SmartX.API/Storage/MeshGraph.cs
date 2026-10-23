namespace SmartX.API.Storage;

// A wireless link between two nodes. Cost = power/signal cost (lower is better).
public record MeshLink(Guid FromId, Guid ToId, double Cost);

// The mesh network as a graph:
//   vertices = sensors (plus one gateway vertex), edges = wireless links.
// Stored as an adjacency list: node id -> list of (neighbour id, link cost).
public class MeshGraph
{
    // The central server that all data must finally reach
    public static readonly Guid GatewayId = new("00000000-0000-0000-0000-000000000001");

    private readonly SensorRegistry _registry;
    private readonly Dictionary<Guid, List<(Guid To, double Cost)>> _adjacency = new();
    private readonly object _lock = new();

    private readonly HashSet<Guid> _down = new();

    public MeshGraph(SensorRegistry registry)
    {
        _registry = registry;
        _adjacency[GatewayId] = new List<(Guid, double)>();
    }

    // Accepts "gateway", a sensor id, or a MAC address and returns the node id
    public bool TryResolve(string? text, out Guid id)
    {
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Trim();

        if (text.Equals("gateway", StringComparison.OrdinalIgnoreCase))
        {
            id = GatewayId;
            return true;
        }

        if (Guid.TryParse(text, out var guid))
        {
            if (guid == GatewayId || _registry.GetById(guid) is not null)
            {
                id = guid;
                return true;
            }
            return false;
        }

        var sensor = _registry.GetByMac(text);
        if (sensor is null) return false;

        id = sensor.Id;
        return true;
    }

    // Returns an error message, or null if the link was added
    public string? AddLink(Guid a, Guid b, double cost)
    {
        if (a == b) return "A node cannot be linked to itself.";
        if (cost <= 0) return "Cost must be greater than 0.";

        lock (_lock)
        {
            if (!_adjacency.ContainsKey(a)) _adjacency[a] = new List<(Guid, double)>();
            if (!_adjacency.ContainsKey(b)) _adjacency[b] = new List<(Guid, double)>();

            if (_adjacency[a].Any(e => e.To == b))
                return "These two nodes are already linked.";

            // Links go both ways
            _adjacency[a].Add((b, cost));
            _adjacency[b].Add((a, cost));
            return null;
        }
    }

    public bool RemoveLink(Guid a, Guid b)
    {
        lock (_lock)
        {
            if (!_adjacency.TryGetValue(a, out var listA) ||
                !_adjacency.TryGetValue(b, out var listB))
                return false;

            int removedA = listA.RemoveAll(e => e.To == b);
            int removedB = listB.RemoveAll(e => e.To == a);
            return removedA > 0 || removedB > 0;
        }
    }

    // Neighbours of one node (a copy, so callers cannot change the graph)
    public List<(Guid To, double Cost)> GetNeighbours(Guid id)
    {
        lock (_lock)
        {
            return _adjacency.TryGetValue(id, out var list)
                ? new List<(Guid, double)>(list)
                : new List<(Guid, double)>();
        }
    }

    // Every link once (the graph stores each one twice, once per direction)
    public List<MeshLink> GetLinks()
    {
        lock (_lock)
        {
            var links = new List<MeshLink>();

            foreach (var (from, edges) in _adjacency)
            {
                foreach (var (to, cost) in edges)
                {
                    if (from.CompareTo(to) < 0)
                        links.Add(new MeshLink(from, to, cost));
                }
            }

            return links;
        }
    }

    // ---------- node health ----------

    // Returns an error message, or null if the node was marked down
    public string? SetDown(Guid id)
    {
        if (id == GatewayId) return "The gateway cannot be marked down.";
        lock (_lock) { _down.Add(id); }
        return null;
    }

    public void SetUp(Guid id)
    {
        lock (_lock) { _down.Remove(id); }
    }

    public bool IsDown(Guid id)
    {
        lock (_lock) { return _down.Contains(id); }
    }

    // ---------- names ----------

    public string NameOf(Guid id) =>
        id == GatewayId ? "Gateway" : _registry.GetById(id)?.Name ?? id.ToString();

    public string? MacOf(Guid id) =>
        id == GatewayId ? null : _registry.GetById(id)?.MacAddress;

    // The gateway plus every registered sensor, even ones with no links
    public List<Guid> GetAllNodeIds()
    {
        var ids = new List<Guid> { GatewayId };
        ids.AddRange(_registry.GetAll().Select(s => s.Id));
        return ids;
    }
}