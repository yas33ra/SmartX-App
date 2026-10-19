using SmartX.API.Storage;

namespace SmartX.API.Endpoints
{
    public record SearchEvent(string Term);

    public static class InsightEndpoints
    {
        public static void MapInsightEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/insights");

            // The form calls this when the USER searches (not on its background refresh)
            group.MapPost("/search", (SearchEvent request, PatternEngine patterns) =>
            {
                var term = request.Term?.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(term))
                {
                    patterns.RecordAction(new ActionEvent(
                        Key: $"search:{term}", Type: ActionType.Search,
                        Label: $"Search for \"{term}\"", SearchTerm: term));
                }
                return Results.NoContent();
            });

            // What the engine suggests right now
            group.MapGet("/suggestions", (PatternEngine patterns, AlertTracker alerts,
                                          CommandService commands, SensorRegistry registry) =>
            {
                var activeAlertKeys = alerts.GetAll().Select(a => $"alert:{a.SensorId}:{a.Kind}");
                var states = commands.GetState().ToDictionary(kv => kv.Key, kv => kv.Value);

                var suggestions = patterns.Suggest(activeAlertKeys)
                    .Where(s => !AlreadyDone(s.Action, states))
                    .Select(s => new
                    {
                        kind = s.Action.Type.ToString(),
                        text = s.Action.Label,
                        searchTerm = s.Action.SearchTerm,
                        on = s.Action.On,
                        sensorIds = s.Action.SensorIds,
                        confidence = Math.Round(s.Confidence * 100),
                        evidence = $"After {Describe(s.ContextKey, registry, patterns)}, " +
                                   $"you did this {s.Count} of {s.Total} times."
                    });

                return Results.Ok(suggestions);
            });
        }

        // Don't suggest "turn Pump A OFF" if Pump A is already off
        private static bool AlreadyDone(ActionEvent action, Dictionary<Guid, bool> states) =>
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
    }
}