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
            await ThrowIfValidationErrorAsync(response);
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
            await ThrowIfValidationErrorAsync(response);
            response.EnsureSuccessStatusCode();
        }

        private static async Task ThrowIfValidationErrorAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var body = await response.Content.ReadFromJsonAsync<ValidationResponse>();
                var message = string.Join(Environment.NewLine, body?.Errors ?? new List<string>());
                throw new InvalidOperationException(message);
            }
        }


        // Devices, with optional search / category / alerts-only filters
        public async Task<List<DeviceInfo>> GetDevicesAsync(string? search, string? category, bool? hasAlerts)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"q={Uri.EscapeDataString(search.Trim())}");
            if (!string.IsNullOrWhiteSpace(category))
                query.Add($"category={Uri.EscapeDataString(category)}");
            if (hasAlerts is not null)
                query.Add($"hasAlerts={hasAlerts.Value.ToString().ToLowerInvariant()}");

            string url = "/devices" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<DeviceInfo>>(url) ?? new List<DeviceInfo>();
        }

        public async Task<AlertsResponse> GetAlertsAsync()
        {
            return await _http.GetFromJsonAsync<AlertsResponse>("/alerts") ?? new AlertsResponse();
        }

        // Newest command first (the API reads the stack from the top)
        public async Task<List<CommandInfo>> GetCommandHistoryAsync()
        {
            return await _http.GetFromJsonAsync<List<CommandInfo>>("/commands/history") ?? new List<CommandInfo>();
        }

        // sensorIds = null means "every control device"
        public async Task<CommandInfo> IssueCommandAsync(bool on, List<Guid>? sensorIds)
        {
            var response = await _http.PostAsJsonAsync("/commands", new IssueCommandRequest(on, sensorIds));
            await ThrowIfValidationErrorAsync(response);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CommandInfo>())!;
        }

        // Reverts the most recent command; the API answers 400 "Nothing to undo." if the stack is empty
        public async Task<CommandInfo?> UndoLastCommandAsync()
        {
            var response = await _http.PostAsync("/commands/undo", null);
            await ThrowIfValidationErrorAsync(response);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UndoResponse>();
            return result?.Command;
        }
    }



    internal class ValidationResponse
    {
        public List<string> Errors { get; set; } = new();
    }
}
