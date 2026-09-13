// a generic Telemetry class that can carry any type of reading 
namespace SmartX.API.Models
{
    public class TelemetryPacket<T>
    {
        public Guid SensorId { get; set; } //identify sensor

        public string SensorCategory { get; set; } = string.Empty; // identify category

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow; //time stamp 

        public T Value { get; set; } = default!; //default value for whatever type is entered
    }

   

}


