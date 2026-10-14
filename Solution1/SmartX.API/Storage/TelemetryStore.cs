// Holds every telemetry packet received, one strongly typed list per sensor category
using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public class TelemetryStore
    {
        private readonly List<TelemetryPacket<double>> _climate = new();
        private readonly List<TelemetryPacket<PowerReading>> _electrical = new();
        private readonly List<TelemetryPacket<bool>> _control = new();

        // The API can receive several requests at once, so we lock while touching the lists
        private readonly object _lock = new();

        public void AddClimate(TelemetryPacket<double> packet)
        {
            lock (_lock) { _climate.Add(packet); }
        }

        public void AddElectrical(TelemetryPacket<PowerReading> packet)
        {
            lock (_lock) { _electrical.Add(packet); }
        }

        public void AddControl(TelemetryPacket<bool> packet)
        {
            lock (_lock) { _control.Add(packet); }
        }

        public List<TelemetryPacket<double>> GetClimate()
        {
            lock (_lock) { return _climate.ToList(); }
        }

        public List<TelemetryPacket<PowerReading>> GetElectrical()
        {
            lock (_lock) { return _electrical.ToList(); }
        }

        public List<TelemetryPacket<bool>> GetControl()
        {
            lock (_lock) { return _control.ToList(); }
        }
    }
}