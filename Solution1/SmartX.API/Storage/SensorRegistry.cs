// This class holds the List<Sensor> in memory 
// This class will also how senors are added and how we find sensors
using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public class SensorRegistry
    {
        private readonly List<Sensor> _sensors = new(); // List (This will be changed to a dictionary in part 2)

    //-----------------------------------------------------------------

        public Sensor Add(Sensor sensor) //Add sensors to the list 
        {
            var hierarchy = sensor.BuildDeploymentHierarchy();

            if (!hierarchy.ValidateHierarchy())
            {
                throw new InvalidOperationException("Sensor deployment hierarchy is invalid or incomplete.");
            }

            _sensors.Add(sensor);
            return sensor;
        }

    //-----------------------------------------------------------------
    
        public List<Sensor> GetAll() // return all registered sensors
        {
            return _sensors;
        }

    //-----------------------------------------------------------------

        public Sensor? GetById(Guid id) // return the first sensor matching the ID
        {
            return _sensors.FirstOrDefault(s => s.Id == id);
        }
    }
}
