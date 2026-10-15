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
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".txt", ".log", ".csv", ".pdf", ".png", ".jpg", ".jpeg" };
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

                    if (file is null || file.Length == 0)
                    {
                        return Results.BadRequest(new { errors = new[] { "Choose a file to upload." } });
                    }

                    if (file.Length > MaxFileSizeBytes)
                    {
                        return Results.BadRequest(new { errors = new[] { "File is too large. The maximum size is 5 MB." } });
                    }

                    var extension = Path.GetExtension(file.FileName);
                    if (!AllowedExtensions.Contains(extension))
                    {
                        return Results.BadRequest(new
                        {
                            errors = new[] { $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}." }
                        });
                    }

                    var uploadsFolder = Path.Combine(AppContext.BaseDirectory, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{id}_{Guid.NewGuid():N}{extension}";
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
