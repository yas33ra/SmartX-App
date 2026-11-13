using SmartX.API.Endpoints;

namespace SmartX.API.Storage
{
    // Runs in the background. Every second it checks whether the suggestions
    // changed and, if so, pushes them to every listening client.
    public class SuggestionWatcher : BackgroundService
    {
        private readonly PatternEngine _patterns;
        private readonly AlertTracker _alerts;
        private readonly CommandService _commands;
        private readonly SuggestionBroadcaster _broadcaster;

        public SuggestionWatcher(PatternEngine patterns, AlertTracker alerts,
                                 CommandService commands, SuggestionBroadcaster broadcaster)
        {
            _patterns = patterns;
            _alerts = alerts;
            _commands = commands;
            _broadcaster = broadcaster;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var alertKeys = _alerts.GetAll()
                        .Select(a => $"alert:{a.SensorId}:{a.Kind}").ToList();
                    var states = _commands.GetState().ToDictionary(kv => kv.Key, kv => kv.Value);

                    if (_patterns.TryGetChangedSuggestions(
                            alertKeys, action => InsightEndpoints.AlreadyDone(action, states), out var current))
                    {
                        _broadcaster.Publish(current);
                    }
                }
                catch (Exception)
                {
                    // one bad check must never stop the watcher
                }
            }
        }
    }
}