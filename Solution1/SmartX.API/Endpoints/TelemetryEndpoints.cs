// Routes for sending in and reading back telemetry, one pair per sensor category
using SmartX.API.Models;
using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    public static class TelemetryEndpoints
    {
        public static void MapTelemetryEndpoints(this WebApplication app)
        {
            //----------------------------------------------------------------------------
            // Climate: the value is a temperature (double)

            app.MapPost("/telemetry/climate",
    (TelemetryPacket<double> packet, SensorRegistry registry, IngestionPipeline pipeline) =>
    {
        var problem = CheckSensor(packet.SensorId, SensorCategory.Climate, registry);
        if (problem is not null)
        {
            return problem;
        }

        var item = new IngestionItem(packet.SensorId, SensorCategory.Climate, packet.Value, packet.TimeStamp);
        bool critical = pipeline.Enqueue(item);

        return Results.Accepted("/telemetry/climate", new { queued = true, critical });
    });

            app.MapGet("/telemetry/climate", (TelemetryStore store) =>
                Results.Ok(store.GetClimate()));

            //----------------------------------------------------------------------------
            // Electrical: the value is a PowerReading (watts)
            // Valid readings go into the ingestion queues; the worker stores them afterwards
            app.MapPost("/telemetry/electrical",
                (TelemetryPacket<PowerReading> packet, SensorRegistry registry, IngestionPipeline pipeline) =>
                {
                    var problem = CheckSensor(packet.SensorId, SensorCategory.Electrical, registry);
                    if (problem is not null)
                    {
                        return problem;
                    }

                    if (packet.Value is null || packet.Value.Watts < 0)
                    {
                        return Results.BadRequest(new { errors = new[] { "Watts must be 0 or more." } });
                    }

                    var item = new IngestionItem(packet.SensorId, SensorCategory.Electrical,
                                                 packet.Value.Watts, packet.TimeStamp);
                    bool critical = pipeline.Enqueue(item);

                    return Results.Accepted("/telemetry/electrical", new { queued = true, critical });
                });

            app.MapGet("/telemetry/electrical", (TelemetryStore store) =>
    Results.Ok(store.GetElectrical()));
            //----------------------------------------------------------------------------
            // Control: the value is on/off (bool)

            app.MapPost("/telemetry/control",
    (TelemetryPacket<bool> packet, SensorRegistry registry, IngestionPipeline pipeline) =>
    {
        var problem = CheckSensor(packet.SensorId, SensorCategory.Control, registry);
        if (problem is not null)
        {
            return problem;
        }

        // bool becomes 1 or 0 to fit the shared queue item; the pipeline converts it back when storing
        var item = new IngestionItem(packet.SensorId, SensorCategory.Control, packet.Value ? 1 : 0, packet.TimeStamp);
        bool critical = pipeline.Enqueue(item);

        return Results.Accepted("/telemetry/control", new { queued = true, critical });
    });

            app.MapGet("/telemetry/control", (TelemetryStore store) =>
                Results.Ok(store.GetControl()));


            app.MapPost("/telemetry/climate/batch",
    (ClimateBatchRequest request, SensorRegistry registry, IngestionPipeline pipeline) =>
    {
        var problem = CheckSensor(request.SensorId, SensorCategory.Climate, registry);
        if (problem is not null)
        {
            return problem;
        }

        if (request.Batches is null || request.Batches.Length == 0)
        {
            return Results.BadRequest(new { errors = new[] { "Send at least one batch of readings." } });
        }

        int totalReadings = request.Batches.Sum(batch => batch?.Length ?? 0);
        if (totalReadings == 0 || totalReadings > 10000)
        {
            return Results.BadRequest(new { errors = new[] { "A upload must contain between 1 and 10000 readings." } });
        }

        var start = request.FirstReadingAt ?? DateTime.UtcNow;
        var interval = TimeSpan.FromSeconds(request.IntervalSeconds > 0 ? request.IntervalSeconds : 60);

        // raw jagged array to typed List of packets
        var packets = ReadingBatchConverter.ToPackets(request.SensorId, request.Batches, start, interval);

        int criticalCount = 0;
        foreach (var packet in packets)
        {
            var item = new IngestionItem(packet.SensorId, SensorCategory.Climate, packet.Value, packet.TimeStamp);
            if (pipeline.Enqueue(item))
            {
                criticalCount++;
            }
        }

        return Results.Accepted("/telemetry/climate", new { received = packets.Count, critical = criticalCount });
    });
        }

        // Shared check: does the sensor exist, and is it the right category for this route?
        // Returns null when all is well, or the error response to send back.
        private static IResult? CheckSensor(Guid sensorId, SensorCategory expected, SensorRegistry registry)
        {
            var sensor = registry.GetById(sensorId);

            if (sensor is null)
            {
                return Results.NotFound(new { errors = new[] { "Sensor not found." } });
            }

            if (sensor.Category != expected)
            {
                return Results.BadRequest(new
                {
                    errors = new[] { $"'{sensor.Name}' is a {sensor.Category} sensor, not {expected}." }
                });
            }

            return null;
        }
    }
}