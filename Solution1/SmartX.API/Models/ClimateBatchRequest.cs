// The body a sensor sends when it uploads a backlog of buffered temperature readings
namespace SmartX.API.Models
{
    public class ClimateBatchRequest
    {
        public Guid SensorId { get; set; }

        // Jagged array: each inner array is one batch, and batches can differ in length
        public float[][] Batches { get; set; } = Array.Empty<float[]>();

        // When the first reading was taken (defaults to now if the sender leaves it out)
        public DateTime? FirstReadingAt { get; set; }

        // Gap between readings, in seconds
        public int IntervalSeconds { get; set; } = 60;
    }
}