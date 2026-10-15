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
                (TelemetryPacket<double> packet, SensorRegistry registry, TelemetryStore store) =>
                {
                    var problem = CheckSensor(packet.SensorId, SensorCategory.Climate, registry);
                    if (problem is not null)
                    {
                        return problem;
                    }

                    store.AddClimate(packet);
                    return Results.Created("/telemetry/climate", packet);
                });

            app.MapGet("/telemetry/climate", (TelemetryStore store) =>
                Results.Ok(store.GetClimate()));

            //----------------------------------------------------------------------------
            // Electrical: the value is a PowerReading (watts)

            app.MapPost("/telemetry/electrical",
                (TelemetryPacket<PowerReading> packet, SensorRegistry registry, TelemetryStore store) =>
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

                    store.AddElectrical(packet);
                    return Results.Created("/telemetry/electrical", packet);
                });

            app.MapGet("/telemetry/electrical", (TelemetryStore store) =>
                Results.Ok(store.GetElectrical()));

            //----------------------------------------------------------------------------
            // Control: the value is on/off (bool)

            app.MapPost("/telemetry/control",
                (TelemetryPacket<bool> packet, SensorRegistry registry, TelemetryStore store) =>
                {
                    var problem = CheckSensor(packet.SensorId, SensorCategory.Control, registry);
                    if (problem is not null)
                    {
                        return problem;
                    }

                    store.AddControl(packet);
                    return Results.Created("/telemetry/control", packet);
                });

            app.MapGet("/telemetry/control", (TelemetryStore store) =>
                Results.Ok(store.GetControl()));


            app.MapPost("/telemetry/climate/batch",
    (ClimateBatchRequest request, SensorRegistry registry, TelemetryStore store) =>
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

        foreach (var packet in packets)
        {
            store.AddClimate(packet);
        }

        return Results.Ok(new { received = packets.Count });
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