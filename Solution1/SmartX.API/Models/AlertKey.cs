namespace SmartX.API.Models
{
    // A record struct gets value-based Equals/GetHashCode for free, so two keys with the
    // same sensor and kind count as the SAME item and the HashSet rejects the second one.
    public readonly record struct AlertKey(Guid SensorId, AlertKind Kind);
}