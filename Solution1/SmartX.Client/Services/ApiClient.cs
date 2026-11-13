//This  tells forms how to construct HTTP requests
using SmartX.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

        public async Task<List<SuggestionInfo>> GetSuggestionsAsync()
        {
            return await _http.GetFromJsonAsync<List<SuggestionInfo>>("/insights/suggestions") ?? new List<SuggestionInfo>();
        }

        public async Task<List<InsightInfo>> GetDeviceInsightsAsync()
        {
            return await _http.GetFromJsonAsync<List<InsightInfo>>("/insights/devices") ?? new List<InsightInfo>();
        }

        // Tells the engine the USER searched for something (so it can learn the pattern)
        public async Task RecordSearchAsync(string term)
        {
            var response = await _http.PostAsJsonAsync("/insights/search", new SearchEventRequest(term));
            response.EnsureSuccessStatusCode();
        }

        // ---------- Network Topology (Part 3) ----------

        // Reads the { "errors": [...] } message the API sends with a 400
        private static async Task<string> ReadApiErrorAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();

            try
            {
                if (JsonNode.Parse(text)?["errors"] is JsonArray errors && errors.Count > 0)
                {
                    return string.Join(" ", errors.Select(e => (string?)e));
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // not JSON: fall through to the generic message
            }

            return $"The API returned {(int)response.StatusCode}.";
        }

        public async Task<MeshInfo> GetMeshAsync() =>
            await _http.GetFromJsonAsync<MeshInfo>("/mesh") ?? new MeshInfo();

        public async Task<MeshStatusInfo> GetMeshStatusAsync() =>
            await _http.GetFromJsonAsync<MeshStatusInfo>("/mesh/status") ?? new MeshStatusInfo();

        public async Task<MstInfo> GetMstAsync() =>
            await _http.GetFromJsonAsync<MstInfo>("/mesh/mst") ?? new MstInfo();

        // node = a MAC address or a sensor id
        public async Task<RouteComparison> GetRouteComparisonAsync(string node)
        {
            var response = await _http.GetAsync($"/mesh/mst/route/{Uri.EscapeDataString(node)}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadApiErrorAsync(response));
            }

            return await response.Content.ReadFromJsonAsync<RouteComparison>() ?? new RouteComparison();
        }

        public async Task SetNodeDownAsync(string node, bool down)
        {
            var action = down ? "down" : "up";
            var response = await _http.PostAsync($"/mesh/nodes/{Uri.EscapeDataString(node)}/{action}", null);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadApiErrorAsync(response));
            }
        }

        // Readings for one sensor in time order, optionally limited to a time window (UTC)
        public async Task<List<HistoryPointInfo>> GetHistoryAsync(
            Guid sensorId, DateTime? sinceUtc = null, DateTime? untilUtc = null, int? limit = null)
        {
            var query = new List<string>();

            if (sinceUtc is not null)
                query.Add("since=" + Uri.EscapeDataString(sinceUtc.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
            if (untilUtc is not null)
                query.Add("until=" + Uri.EscapeDataString(untilUtc.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
            if (limit is > 0)
                query.Add("limit=" + limit.Value);

            var url = $"/history/{sensorId}" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            return await _http.GetFromJsonAsync<List<HistoryPointInfo>>(url) ?? new List<HistoryPointInfo>();
        }

        public async Task<HistoryStatsInfo> GetHistoryStatsAsync(Guid sensorId) =>
            await _http.GetFromJsonAsync<HistoryStatsInfo>($"/history/{sensorId}/stats") ?? new HistoryStatsInfo();

        // fromNode / toNode can be a sensor id, a MAC address or "gateway"
        public async Task AddLinkAsync(string fromNode, string toNode, double cost)
        {
            var response = await _http.PostAsJsonAsync("/mesh/links",
                new { From = fromNode, To = toNode, Cost = cost });

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadApiErrorAsync(response));
            }
        }

        public async Task RemoveLinkAsync(string fromNode, string toNode)
        {
            var response = await _http.DeleteAsync(
                $"/mesh/links?from={Uri.EscapeDataString(fromNode)}&to={Uri.EscapeDataString(toNode)}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadApiErrorAsync(response));
            }
        }

        // Opens the server's event stream and hands back a new suggestion list every time one is pushed
        public async IAsyncEnumerable<List<SuggestionInfo>> StreamSuggestionsAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "insights/suggestions/stream");

            // ResponseHeadersRead = give me the response as soon as it starts, do not wait for it to end
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) yield break;                 // the server closed the connection

                if (line.StartsWith("data: "))                 // the "event:" and blank lines are skipped
                {
                    var list = JsonSerializer.Deserialize<List<SuggestionInfo>>(
                        line["data: ".Length..], JsonSerializerOptions.Web);

                    if (list is not null) yield return list;
                }
            }
        }

        public async Task<List<string>> GetSearchHistoryAsync()
        {
            return await _http.GetFromJsonAsync<List<string>>("insights/search-history") ?? new List<string>();
        }
    }



    internal class ValidationResponse
    {
        public List<string> Errors { get; set; } = new();
    }
}
