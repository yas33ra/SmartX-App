// turns raw batches of readings (a jagged array) into typed telemetry packets
using SmartX.API.Models;

namespace SmartX.API.Storage
{
    public class ReadingBatchConverter
    {
        public static List<TelemetryPacket<double>> ToPackets(
            Guid sensorId, float[][] batches, DateTime firstReadingAt, TimeSpan interval)
        {
            var packets = new List<TelemetryPacket<double>>();
            var time = firstReadingAt;

            foreach (var batch in batches)
            {
                if (batch is null)
                {
                    continue;
                }

                foreach (var raw in batch)
                {
                    if (!float.IsFinite(raw))
                    {
                        continue;
                    }

                    packets.Add(new TelemetryPacket<double>
                    {
                        SensorId = sensorId,
                        TimeStamp = time,
                        Value = Math.Round((double)raw, 2)
                    });

                    time += interval;
                }
            }

            return packets;
        }
    }
}
