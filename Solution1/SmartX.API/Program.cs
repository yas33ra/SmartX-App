using SmartX.API.Endpoints;
using SmartX.API.Models;
using SmartX.API.Storage;

namespace SmartX.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<SensorRegistry>(); // one instance of SensorRegistry

            builder.Services.AddSingleton<TelemetryStore>();

            var app = builder.Build();

            app.MapSensorEndpoints ();

            app.MapTelemetryEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();
        }
    }
}
