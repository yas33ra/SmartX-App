//copy of Sensor from SmartX.API to send&recieve JSON correctly
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartX.Client.Models
{
    public class Sensor
    {
        public Guid Id { get; set; }  // Why You Should Use GUIDs Instead of Integers for IDs https://medium.com/@uzonweze/why-you-should-use-guids-instead-of-integers-for-ids-62addafb2aa4 

        public string Name { get; set; } = string.Empty;

        public string MacAddress { get; set; } = string.Empty;

        public string Facility { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string SubZone { get; set; } = string.Empty;

        public SensorCategory Category { get; set; } 

        public string? AttachedFilePath { get; set; }

        public DateTime RegisteredAt { get; set; }
    }
}
