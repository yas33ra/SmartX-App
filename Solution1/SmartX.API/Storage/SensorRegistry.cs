// This class holds the sensors in memory using two Dictionaries:
//   _byMac keyed by MAC address (how a device identifies itself)
//   _byId  keyed by Guid        (what telemetry packets carry)
// Both give O(1) lookups instead of scanning a List
using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public class SensorRegistry
    {
        private readonly Dictionary<string, Sensor> _byMac = new();
        private readonly Dictionary<Guid, Sensor> _byId = new();

        // The API handles several requests at once, so we lock while touching the dictionaries
        private readonly object _lock = new();

        //-----------------------------------------------------------------

        // "aa-bb-cc-dd-ee-ff" and "AA:BB:CC:DD:EE:FF" are the same device,
        // so every MAC is converted to one standard form before it is used as a key
        public static string NormalizeMac(string mac) =>
            mac.Trim().ToUpperInvariant().Replace('-', ':');

        //-----------------------------------------------------------------

        public Sensor Add(Sensor sensor) // Add a sensor to both dictionaries
        {
            var hierarchy = sensor.BuildDeploymentHierarchy();

            if (!hierarchy.ValidateHierarchy())
            {
                throw new InvalidOperationException("Sensor deployment hierarchy is invalid or incomplete.");
            }

            var key = NormalizeMac(sensor.MacAddress);

            lock (_lock)
            {
                if (_byMac.ContainsKey(key))
                {
                    throw new InvalidOperationException("A sensor with this MAC address is already registered.");
                }

                _byMac[key] = sensor;
                _byId[sensor.Id] = sensor;
                return sensor;
            }
        }

        //-----------------------------------------------------------------

        public List<Sensor> GetAll() // return a copy of all registered sensors, in registration order
        {
            lock (_lock)
            {
                // Dictionaries do not guarantee order, so we sort for a stable list in the UI
                return _byMac.Values.OrderBy(s => s.RegisteredAt).ToList();
            }
        }

        //-----------------------------------------------------------------

        public Sensor? GetById(Guid id) // O(1) lookup by ID
        {
            lock (_lock)
            {
                return _byId.TryGetValue(id, out var sensor) ? sensor : null;
            }
        }

        //-----------------------------------------------------------------

        public Sensor? GetByMac(string mac) // O(1) lookup by MAC (used when a device sends data)
        {
            lock (_lock)
            {
                return _byMac.TryGetValue(NormalizeMac(mac), out var sensor) ? sensor : null;
            }
        }

        //-----------------------------------------------------------------

        public void UpdateAttachedFile(Guid id, string filePath) // updates the object in place
        {
            lock (_lock)
            {
                if (_byId.TryGetValue(id, out var sensor))
                {
                    sensor.AttachedFilePath = filePath;
                }
            }
        }

        //-----------------------------------------------------------------

        // Check for duplicate MAC
        public bool MacExists(string mac)
        {
            lock (_lock)
            {
                return _byMac.ContainsKey(NormalizeMac(mac));
            }
        }
    }
}