namespace SmartX.API.Storage
{
    public enum ActionType { Search, Command }

    // Something the user did. Key identifies "the same action" each time it recurs.
    public record ActionEvent(string Key, ActionType Type, string Label,
                              string? SearchTerm = null, bool? On = null, List<Guid>? SensorIds = null);

    // A suggestion: "in this context you did this Count times out of Total"
    public record PatternSuggestion(ActionEvent Action, string ContextKey, int Count, int Total)
    {
        public double Confidence => (double)Count / Total;
    }

    // Learns which user actions tend to follow which events (a simple Markov chain)
    public class PatternEngine
    {
        private const int MinCount = 2;            // a pattern must have happened at least twice
        private const double MinConfidence = 0.4;  // and make up at least 40% of the follow-ups
        private static readonly TimeSpan ResponseWindow = TimeSpan.FromMinutes(10);

        // "what happened" -> ( "what the user did next" -> how many times )
        private readonly Dictionary<string, Dictionary<string, int>> _transitions = new();

        // Remembers the details of every action seen, so a key can be turned back into something runnable
        private readonly Dictionary<string, ActionEvent> _actions = new();

        private readonly object _lock = new();
        private string? _previousKey;
        private DateTime _previousAt;

        // A system event (such as a new alert). It can lead to a user action but is not one itself.
        public void RecordTrigger(string key)
        {
            lock (_lock)
            {
                _previousKey = key;
                _previousAt = DateTime.UtcNow;
            }
        }

        // A user action. If it quickly followed something, count that as a pattern.
        public void RecordAction(ActionEvent action)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                _actions[action.Key] = action;

                if (_previousKey is not null && _previousKey != action.Key && now - _previousAt <= ResponseWindow)
                {
                    if (!_transitions.TryGetValue(_previousKey, out var next))
                    {
                        next = new Dictionary<string, int>();
                        _transitions[_previousKey] = next;
                    }
                    next[action.Key] = next.GetValueOrDefault(action.Key) + 1;
                }

                _previousKey = action.Key;
                _previousAt = now;
            }
        }

        // activeTriggerKeys = what is going on right now (for example, the alerts currently active)
        public List<PatternSuggestion> Suggest(IEnumerable<string> activeTriggerKeys, int max = 3)
        {
            lock (_lock)
            {
                var contexts = activeTriggerKeys.ToList();
                if (_previousKey is not null)
                {
                    contexts.Add(_previousKey);   // also: what the user did most recently
                }

                var best = new Dictionary<string, PatternSuggestion>();

                foreach (var context in contexts.Distinct())
                {
                    if (!_transitions.TryGetValue(context, out var next)) continue;

                    int total = next.Values.Sum();

                    foreach (var (targetKey, count) in next)
                    {
                        if (count < MinCount) continue;
                        if ((double)count / total < MinConfidence) continue;
                        if (targetKey == _previousKey) continue;     // they literally just did this

                        var suggestion = new PatternSuggestion(_actions[targetKey], context, count, total);

                        // Same action suggested by two contexts: keep the more confident one
                        if (!best.TryGetValue(targetKey, out var existing) || suggestion.Confidence > existing.Confidence)
                        {
                            best[targetKey] = suggestion;
                        }
                    }
                }

                return best.Values
                           .OrderByDescending(s => s.Confidence)
                           .ThenByDescending(s => s.Count)
                           .Take(max)
                           .ToList();
            }
        }

        // Used to describe an action key in plain words
        public string? GetLabel(string key)
        {
            lock (_lock) { return _actions.TryGetValue(key, out var a) ? a.Label : null; }
        }
    }
}