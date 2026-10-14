//This class defines the API routes for registering sensoors, finging sensors and uploading files
// Using Minimal APIs instead of Controller based APIs 

using SmartX.API.Models;
using SmartX.API.Storage;
using SmartX.API.Validation;
using System.Runtime.CompilerServices;

namespace SmartX.API.Endpoints
{
    public static class SensorEndpoints
    {
        public static void MapSensorEndpoints (this WebApplication app)
        {

            //----------------------------------------------------------------------------

            app.MapPost("/sensors", (Sensor sensor, SensorRegistry registry) =>
            {
                var errors = SensorValidator.Validate(sensor);

                if (errors.Count == 0 && registry.MacExists(sensor.MacAddress))
                    errors.Add("A sensor with this MAC address is already registered.");

                if (errors.Count > 0)
                    return Results.BadRequest(new { errors });

                sensor.Id = Guid.NewGuid();
                sensor.RegisteredAt = DateTime.UtcNow;

                var created = registry.Add(sensor);
                return Results.Created($"/sensors/{created.Id}", created);
            });


            //----------------------------------------------------------------------------

            app.MapGet("/sensors", (SensorRegistry registry) =>
            {
                return Results.Ok(registry.GetAll());
            });


        //----------------------------------------------------------------------------

            app.MapGet("/sensors/{id}", (Guid id, SensorRegistry registry) =>
                {
                    var sensor = registry.GetById(id);
                    return sensor is not null ? Results.Ok(sensor) : Results.NotFound();
                });


            //----------------------------------------------------------------------------


            app.MapPost("/sensors/{id}/upload", async (Guid id, IFormFile file, SensorRegistry registry) =>
                {
                var sensor = registry.GetById(id);
                    if (sensor is null)
                    {
                        return Results.NotFound();
                    }

                    var uploadsFolder = Path.Combine(AppContext.BaseDirectory, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{id}_{file.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create)) 
                    {
                    
                        await file.CopyToAsync(stream);
                    }

                    registry.UpdateAttachedFile(id, filePath);

                    return Results.Ok(sensor);


                })
                .DisableAntiforgery();

        }
    }
}
