//This class defines the aspects and information of each sensor that we will store 
namespace SmartX.API.Models
{
    public class Sensor
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Why You Should Use GUIDs Instead of Integers for IDs https://medium.com/@uzonweze/why-you-should-use-guids-instead-of-integers-for-ids-62addafb2aa4 

        public string MacAddress { get; set; } = string .Empty;

        public string Location { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string? AttachedFilePath { get; set; } 

        public DateTime RegistererAt { get; set; } = DateTime.UtcNow;
    }
}
