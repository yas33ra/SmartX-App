namespace SmartX.API.Models
{
    // One reading waiting to be processed. Value is a double so one queue can carry
    // every category; it is converted back to the typed packet when stored.
    public record IngestionItem(Guid SensorId, SensorCategory Category, double Value, DateTime TimeStamp);
}