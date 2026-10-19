// SmartX device simulator: behaves like real hardware by calling the API over HTTP.
// Start SmartX.API first, then run:  dotnet run --project SmartX.Simulator
using System.Net.Http.Json;
using System.Text.Json.Nodes;

string baseUrl = args.Length > 0 ? args[0] : "http://localhost:5056";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var random = new Random();

var fleet = new List<SimDevice>();
var offline = new HashSet<Guid>();                    // devices that have "lost power" and send nothing
var holdUntil = new Dictionary<Guid, DateTime>();     // devices holding a scripted reading for a few seconds
var climateValue = new Dictionary<Guid, double>();    // each climate sensor drifts around 22
bool paused = false;

Console.WriteLine($"SmartX device simulator -> {baseUrl}");

// The fleet is described in devices.json
var config = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "devices.json")))!;
var template = config["template"]!.AsObject();
var definitions = config["devices"]!.AsArray();

try
{
    await RegisterFleetAsync();
}
catch (HttpRequestException)
{
    Console.WriteLine($"Cannot reach the API at {baseUrl}. Start SmartX.API first.");
    return;
}

if (fleet.Count == 0)
{
    Console.WriteLine("No devices available. Fix the errors above and run again.");
    return;
}

Console.WriteLine();
Console.WriteLine("Keys:  S power spike | N near-limit power | H heat spike | U unusual reading");
Console.WriteLine("       D disconnect Greenhouse | R reset alerts | P pause/resume ingestion");
Console.WriteLine("       T teach demo pattern | Q quit");
Console.WriteLine();

while (true)
{
    try
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Q) return;
            await HandleKeyAsync(key);
        }

        int sent = await SendRegularReadingsAsync();
        await PrintStatusAsync(sent);
    }
    catch (HttpRequestException)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] API not reachable, retrying...");
    }

    await Task.Delay(2000);
}

//---------------------------------------------------------------------------
// Registration

string NormalizeMac(string mac) => mac.Trim().ToUpperInvariant().Replace('-', ':');

// What is already registered, so re-running the simulator never creates duplicates
async Task<Dictionary<string, Guid>> GetExistingAsync()
{
    var existing = new Dictionary<string, Guid>();
    var all = await http.GetFromJsonAsync<JsonArray>("/sensors") ?? new JsonArray();
    foreach (var s in all)
    {
        var mac = (string?)s?["macAddress"];
        var id = (Guid?)s?["id"];
        if (mac is not null && id is not null)
        {
            existing[NormalizeMac(mac)] = id.Value;
        }
    }
    return existing;
}

async Task RegisterFleetAsync()
{
    var existing = await GetExistingAsync();

    foreach (var def in definitions)
    {
        string name = (string)def!["name"]!;
        string mac = (string)def["macAddress"]!;
        string kind = (string)def["simKind"]!;
        string key = NormalizeMac(mac);

        if (existing.TryGetValue(key, out var existingId))
        {
            fleet.Add(new SimDevice(name, existingId, kind));
            Console.WriteLine($"  reuse     {name}");
            continue;
        }

        // Start from the template (a working register body) and override the three identifying fields
        var body = template.DeepClone().AsObject();
        body["name"] = name;
        body["macAddress"] = mac;
        body["category"] = def["category"]!.DeepClone();

        var response = await http.PostAsJsonAsync("/sensors", body);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"  FAILED    {name}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            continue;
        }

        existing = await GetExistingAsync();
        if (existing.TryGetValue(key, out var newId))
        {
            fleet.Add(new SimDevice(name, newId, kind));
            Console.WriteLine($"  register  {name}");
        }
    }
}

//---------------------------------------------------------------------------
// Regular traffic

async Task<int> SendRegularReadingsAsync()
{
    int sent = 0;

    foreach (var d in fleet)
    {
        if (offline.Contains(d.Id)) continue;                                   // powered off: silent
        if (holdUntil.TryGetValue(d.Id, out var until) && DateTime.UtcNow < until) continue;

        if (d.Kind == "climate")
        {
            double v = climateValue.GetValueOrDefault(d.Id, 22.0);
            v = Math.Clamp(v + (random.NextDouble() - 0.5) * 0.6, 20.0, 24.0);   // gentle drift
            climateValue[d.Id] = v;
            await http.PostAsJsonAsync("/telemetry/climate", new { sensorId = d.Id, value = Math.Round(v, 2) });
            sent++;
        }
        else if (d.Kind == "electrical")
        {
            int watts = 800 + random.Next(-120, 121);
            await http.PostAsJsonAsync("/telemetry/electrical", new { sensorId = d.Id, value = new { watts } });
            sent++;
        }
        // control devices only report when commanded
    }

    return sent;
}

async Task PrintStatusAsync(int sent)
{
    var status = await http.GetFromJsonAsync<JsonNode>("/ingestion/status");
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] sent {sent,2} | queued: standard={status?["standardQueued"]} " +
                      $"critical={status?["criticalQueued"]} | processed={status?["processed"]} | offline={offline.Count}" +
                      (paused ? " | PAUSED" : ""));
}

//---------------------------------------------------------------------------
// Scripted events (keys)

async Task SendElectricalAsync(int watts, string label)
{
    var device = fleet.FirstOrDefault(d => d.Kind == "electrical");
    if (device is null) { Console.WriteLine("  >> no electrical device in devices.json"); return; }

    var response = await http.PostAsJsonAsync("/telemetry/electrical", new { sensorId = device.Id, value = new { watts } });
    holdUntil[device.Id] = DateTime.UtcNow.AddSeconds(8);   // keep this reading visible for a few seconds
    Console.WriteLine($"  >> {device.Name}: {label}  [{(int)response.StatusCode}]");
}

async Task SendClimateAsync(double value, string label)
{
    var device = fleet.FirstOrDefault(d => d.Kind == "climate");
    if (device is null) { Console.WriteLine("  >> no climate device in devices.json"); return; }

    var response = await http.PostAsJsonAsync("/telemetry/climate", new { sensorId = device.Id, value });
    holdUntil[device.Id] = DateTime.UtcNow.AddSeconds(8);
    Console.WriteLine($"  >> {device.Name}: {label}  [{(int)response.StatusCode}]");
}

async Task ResetAlertsAsync()
{
    foreach (var d in fleet)
    {
        foreach (var kind in new[] { "Disconnected", "PowerSpike", "ClimateExtreme" })
        {
            await http.DeleteAsync($"/alerts/{d.Id}/{kind}");     // 404 just means "wasn't active"
        }
    }
    offline.Clear();
}

async Task HandleKeyAsync(ConsoleKey key)
{
    switch (key)
    {
        case ConsoleKey.S:
            await SendElectricalAsync(6000, "POWER SPIKE 6000 W (critical)");
            break;

        case ConsoleKey.N:
            await SendElectricalAsync(4200, "near-limit 4200 W");
            break;

        case ConsoleKey.H:
            await SendClimateAsync(55.0, "HEAT SPIKE 55 (critical)");
            break;

        case ConsoleKey.U:
            await SendClimateAsync(30.0, "unusual reading 30 (needs ~20 s of normal data first)");
            break;

        case ConsoleKey.D:
            {
                var target = fleet.FirstOrDefault(d => d.Kind == "climate");
                if (target is null) break;

                offline.Add(target.Id);
                var response = await http.PostAsJsonAsync("/alerts/disconnect", new { sensorId = target.Id });
                Console.WriteLine($"  >> {target.Name} disconnected [{(int)response.StatusCode}] {await response.Content.ReadAsStringAsync()}");
                break;
            }

        case ConsoleKey.R:
            await ResetAlertsAsync();
            Console.WriteLine("  >> alerts cleared, all devices back online");
            break;

        case ConsoleKey.P:
            paused = !paused;
            await http.PostAsync(paused ? "/ingestion/pause" : "/ingestion/resume", null);
            Console.WriteLine(paused ? "  >> ingestion PAUSED: the queues will fill up" : "  >> ingestion RESUMED");
            break;

        case ConsoleKey.T:
            await TeachDemoPatternAsync();
            break;
    }
}

// Seeds the suggestion engine with two rounds of "power spike, then Pump A OFF" so a demo
// can show a suggestion straight away. This is demo seeding, not real user history.
async Task TeachDemoPatternAsync()
{
    var pump = fleet.FirstOrDefault(d => d.Kind == "control");
    if (pump is null) { Console.WriteLine("  >> no control device in devices.json"); return; }

    Console.WriteLine("  >> teaching: spike, then Pump OFF (twice)...");

    for (int round = 1; round <= 2; round++)
    {
        await ResetAlertsAsync();
        await SendElectricalAsync(6000, $"round {round}: spike");
        await Task.Delay(1500);                                                           // let the worker raise the alert
        await http.PostAsJsonAsync("/commands", new { on = false, sensorIds = new[] { pump.Id } });
        await http.PostAsJsonAsync("/commands", new { on = true, sensorIds = new[] { pump.Id } });   // reset the pump
    }

    await ResetAlertsAsync();
    await SendElectricalAsync(6000, "final spike");
    Console.WriteLine("  >> done: check Suggested Actions in the client within a few seconds");
}

record SimDevice(string Name, Guid Id, string Kind);