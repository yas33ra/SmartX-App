//This class defines the aspects and information of each sensor that we will store 
namespace SmartX.API.Models
{
    public class Sensor
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Why You Should Use GUIDs Instead of Integers for IDs https://medium.com/@uzonweze/why-you-should-use-guids-instead-of-integers-for-ids-62addafb2aa4 

        public string Name { get; set; } = string.Empty;

        public string MacAddress { get; set; } = string .Empty;

        public string Facility { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string SubZone { get; set; } = string.Empty;

        public SensorCategory Category { get; set; } 

        public string? AttachedFilePath { get; set; } 

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;


        public DeploymentLocation BuildDeploymentHierarchy()
        {
            var facilityNode = new DeploymentLocation { Name = Facility, IsTopLevel = true };
            var zoneNode = new DeploymentLocation { Name = Zone, Parent = facilityNode };
            var subZoneNode = new DeploymentLocation { Name = SubZone, Parent = zoneNode };

            return subZoneNode;
        }
    }
}
