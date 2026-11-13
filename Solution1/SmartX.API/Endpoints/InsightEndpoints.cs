using SmartX.API.Storage;
using System.Text.Json;

namespace SmartX.API.Endpoints
{
    public record SearchEvent(string Term);

    public static class InsightEndpoints
    {
        public static void MapInsightEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/insights");

            // The form calls this when the USER searches (not on its background refresh)
            group.MapPost("/search", (SearchEvent request, PatternEngine patterns, SearchHistory history) =>
            {
                var term = request.Term?.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(term))
                {
                    history.Add(term);   // the recent-searches queue

                    patterns.RecordAction(new ActionEvent(
                        Key: $"search:{term}", Type: ActionType.Search,
                        Label: $"Search for \"{term}\"", SearchTerm: term));
                }
                return Results.NoContent();
            });

            // The last few searches, newest first
            group.MapGet("/search-history", (SearchHistory history) =>
                Results.Ok(history.GetRecent()));

            // What the engine suggests right now
            group.MapGet("/suggestions", (PatternEngine patterns, AlertTracker alerts,
                                          CommandService commands, SensorRegistry registry) =>
            {
                var activeAlertKeys = alerts.GetAll().Select(a => $"alert:{a.SensorId}:{a.Kind}");
                var states = commands.GetState().ToDictionary(kv => kv.Key, kv => kv.Value);

                var suggestions = patterns.Suggest(activeAlertKeys)
                    .Where(s => !AlreadyDone(s.Action, states))
                    .Select(s => ToDto(s, registry, patterns));

                return Results.Ok(suggestions);
            });

           

            // Devices that look problematic right now
            group.MapGet("/devices", (InsightEngine engine) =>
                Results.Ok(engine.Analyse().Select(i => new
                {
                    sensorId = i.SensorId,
                    sensorName = i.SensorName,
                    rule = i.Rule,
                    severity = i.Severity.ToString(),
                    message = i.Message
                })));

            // Server-Sent Events: the connection stays open and the server writes to it when suggestions change
            group.MapGet("/suggestions/stream", async (HttpContext http, SuggestionBroadcaster broadcaster,
                                                       PatternEngine patterns, SensorRegistry registry,
                                                       CancellationToken ct) =>
            {
                http.Response.Headers.ContentType = "text/event-stream";
                http.Response.Headers.CacheControl = "no-cache";

                var (id, reader) = broadcaster.Subscribe();

                try
                {
                    // Waits (without blocking a thread) until the next list arrives
                    await foreach (var list in reader.ReadAllAsync(ct))
                    {
                        var json = JsonSerializer.Serialize(
                            list.Select(s => ToDto(s, registry, patterns)), JsonSerializerOptions.Web);

                        await http.Response.WriteAsync($"event: suggestions\ndata: {json}\n\n", ct);
                        await http.Response.Body.FlushAsync(ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    // the client closed the page: normal
                }
                finally
                {
                    broadcaster.Unsubscribe(id);
                }
            });
        }

        // Don't suggest "turn Pump A OFF" if Pump A is already off
        public static bool AlreadyDone(ActionEvent action, Dictionary<Guid, bool> states) =>
            action.On is not null
            && action.SensorIds is not null
            && action.SensorIds.All(id => states.GetValueOrDefault(id, false) == action.On.Value);

        // Turns an internal key into words the operator understands
        private static string Describe(string key, SensorRegistry registry, PatternEngine patterns)
        {
            if (key.StartsWith("alert:"))
            {
                var parts = key.Split(':', 3);                       // alert : sensorId : kind
                var name = Guid.TryParse(parts[1], out var id) ? registry.GetById(id)?.Name : null;
                return $"{name ?? "a device"}'s {parts[2]} alert";
            }
            if (key.StartsWith("search:"))
            {
                return $"you searched \"{key["search:".Length..]}\"";
            }
            return $"\"{patterns.GetLabel(key) ?? key}\"";
        }

        private static object ToDto(PatternSuggestion s, SensorRegistry registry, PatternEngine patterns) => new
        {
            kind = s.Action.Type.ToString(),
            text = s.Action.Label,
            searchTerm = s.Action.SearchTerm,
            on = s.Action.On,
            sensorIds = s.Action.SensorIds,
            confidence = Math.Round(s.Confidence * 100),
            evidence = $"After {Describe(s.ContextKey, registry, patterns)}, " +
               $"you did this {s.Count} of {s.Total} times."
        };



    }
}