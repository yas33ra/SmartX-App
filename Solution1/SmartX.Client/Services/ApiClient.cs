//This  tells forms how to construct HTTP requests
using SmartX.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SmartX.Client.Services
{
    internal class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient()
        {
            _http = new HttpClient
            {
                BaseAddress =  new Uri("http://localhost:5056")
            };
        }

        public async Task<Sensor?> RegisterSensorAsync(Sensor sensor)
        {
            var response = await _http.PostAsJsonAsync("/sensors", sensor);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var body = await response.Content.ReadFromJsonAsync<ValidationResponse>();
                var message = string.Join(Environment.NewLine, body?.Errors ?? new List<string>());
                throw new InvalidOperationException(message);
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Sensor>();
        }
        public async Task<List<Sensor>> GetAllSensorsAsync()
        {
            var sensors = await _http.GetFromJsonAsync<List<Sensor>>("/sensors");
            return sensors ?? new List<Sensor>();
        }

        public async Task UploadFileAsync(Guid sensorId, string filePath)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            using var fileContent = new StreamContent(fileStream);

            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await _http.PostAsync($"/sensors/{sensorId}/upload", content);
            response.EnsureSuccessStatusCode();
        }
    }

    internal class ValidationResponse
    {
        public List<string> Errors { get; set; } = new();
    }
}
