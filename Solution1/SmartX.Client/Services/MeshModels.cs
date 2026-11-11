namespace SmartX.Client.Services;

public class MeshNodeInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? MacAddress { get; set; }
    public string Category { get; set; } = "";
    public bool IsGateway { get; set; }
    public bool IsDown { get; set; }
    public int LinkCount { get; set; }

    public override string ToString() => Name;
}

public class MeshLinkInfo
{
    public Guid FromId { get; set; }
    public string FromName { get; set; } = "";
    public Guid ToId { get; set; }
    public string ToName { get; set; } = "";
    public double Cost { get; set; }
}

public class MeshInfo
{
    public List<MeshNodeInfo> Nodes { get; set; } = new();
    public List<MeshLinkInfo> Links { get; set; } = new();
}

// One connected node from /mesh/status
public class MeshConnectedInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int Hops { get; set; }
    public string NextHop { get; set; } = "";
}

// A down or cut-off node from /mesh/status
public class MeshNameInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? MacAddress { get; set; }
}

public class MeshStatusInfo
{
    public List<MeshConnectedInfo> Connected { get; set; } = new();
    public List<MeshNameInfo> Down { get; set; } = new();
    public List<MeshNameInfo> CutOff { get; set; } = new();
}

public class MstEdgeInfo
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public double Cost { get; set; }
}

public class MstInfo
{
    public double TotalCost { get; set; }
    public int EdgeCount { get; set; }
    public List<MstEdgeInfo> Edges { get; set; } = new();
}

public class RouteDetail
{
    public string Path { get; set; } = "";
    public int Hops { get; set; }
    public double Cost { get; set; }
}

// Cheapest (MST) route next to the fewest-hops (BFS) route for one node
public class RouteComparison
{
    public string Node { get; set; } = "";
    public bool Reachable { get; set; }
    public string? Message { get; set; }
    public RouteDetail? PowerEfficientRoute { get; set; }
    public RouteDetail? FewestHopsRoute { get; set; }
}

public class HistoryPointInfo
{
    public DateTime TimeStamp { get; set; }
    public double Value { get; set; }
}

public class HistoryStatsInfo
{
    public int Count { get; set; }
    public int TreeHeight { get; set; }
    public int IdealHeight { get; set; }
}