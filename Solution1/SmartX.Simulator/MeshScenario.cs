using System.Net.Http.Json;
using System.Text.Json.Nodes;

// Builds the mesh network from devices.json and lets the demo take nodes down.
// Uses only the API over HTTP, like the rest of the simulator.
internal static class MeshScenario
{
    private const string BaseUrl = "http://localhost:5056";
    private static readonly HttpClient Http = new();

    // Nodes taken down by the K key, in order: Mains Meter first, then Greenhouse Temp
    private static readonly string[] DownOrder = { "AA:BB:CC:00:00:03", "AA:BB:CC:00:00:01" };
    private static int _downCount;

    // Reads "links" from devices.json and adds them to the API
    public static async Task BuildAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "devices.json");
        var config = JsonNode.Parse(await File.ReadAllTextAsync(path));

        if (config?["links"] is not JsonArray links || links.Count == 0)
        {
            Console.WriteLine("mesh: no 'links' found in devices.json");
            return;
        }

        int added = 0, existing = 0, failed = 0;

        foreach (var link in links)
        {
            var body = new
            {
                from = (string?)link?["from"],
                to = (string?)link?["to"],
                cost = (double?)link?["cost"] ?? 0
            };

            var response = await Http.PostAsJsonAsync($"{BaseUrl}/mesh/links", body);

            if (response.IsSuccessStatusCode)
            {
                added++;
                continue;
            }

            var text = await response.Content.ReadAsStringAsync();

            if (text.Contains("already linked"))
            {
                existing++;
            }
            else
            {
                failed++;
                Console.WriteLine($"  FAILED {body.from} - {body.to}: {(int)response.StatusCode} {text}");
            }
        }

        Console.WriteLine($"mesh: {added} added, {existing} already there, {failed} failed");
        await PrintSummaryAsync();
    }

    // K key: take the next planned node down (Mains Meter, then Greenhouse Temp)
    public static async Task TakeNextNodeDownAsync()
    {
        if (_downCount >= DownOrder.Length)
        {
            Console.WriteLine("mesh: all planned nodes are already down. Press L to restore them.");
            return;
        }

        var mac = DownOrder[_downCount++];
        var response = await Http.PostAsync($"{BaseUrl}/mesh/nodes/{mac}/down", null);

        Console.WriteLine(response.IsSuccessStatusCode
            ? $"mesh: node {mac} is now DOWN"
            : $"mesh: FAILED {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");

        await PrintSummaryAsync();
    }

    // L key: bring every node back up
    public static async Task RestoreAllAsync()
    {
        foreach (var mac in DownOrder)
        {
            await Http.PostAsync($"{BaseUrl}/mesh/nodes/{mac}/up", null);
        }

        _downCount = 0;
        Console.WriteLine("mesh: all nodes restored");
        await PrintSummaryAsync();
    }

    // One short picture of the mesh: who is connected, down, cut off, and the cheapest tree cost
    public static async Task PrintSummaryAsync()
    {
        var status = JsonNode.Parse(await Http.GetStringAsync($"{BaseUrl}/mesh/status"));
        var mst = JsonNode.Parse(await Http.GetStringAsync($"{BaseUrl}/mesh/mst"));

        static string Join(JsonNode? array, Func<JsonNode?, string> format) =>
            array is JsonArray items && items.Count > 0
                ? string.Join(", ", items.Select(format))
                : "none";

        Console.WriteLine($"  connected : {Join(status?["connected"], n => $"{(string?)n?["name"]} ({(int?)n?["hops"]} hops)")}");
        Console.WriteLine($"  down      : {Join(status?["down"], n => (string?)n?["name"] ?? "?")}");
        Console.WriteLine($"  cut off   : {Join(status?["cutOff"], n => (string?)n?["name"] ?? "?")}");
        Console.WriteLine($"  cheapest tree cost: {mst?["totalCost"]}");
    }
}