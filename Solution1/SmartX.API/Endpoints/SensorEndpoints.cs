//This class defines the API routes for registering sensoors, finging sensors and uploading files
// Using Minimal APIs instead of Controller based APIs 

using SmartX.API.Models;
using SmartX.API.Storage;
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
        }
    }
}
