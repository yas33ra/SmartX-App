using System.Collections.Concurrent;
using System.Threading.Channels;

namespace SmartX.API.Storage
{
    // Keeps one Channel (a small async queue) per connected client
    // and writes every change into all of them.
    public class SuggestionBroadcaster
    {
        private readonly ConcurrentDictionary<Guid, Channel<List<PatternSuggestion>>> _subscribers = new();
        private volatile List<PatternSuggestion> _last = new();

        public (Guid Id, ChannelReader<List<PatternSuggestion>> Reader) Subscribe()
        {
            // Keep at most 5 waiting messages; if a client is slow, drop the oldest
            var channel = Channel.CreateBounded<List<PatternSuggestion>>(
                new BoundedChannelOptions(5) { FullMode = BoundedChannelFullMode.DropOldest });

            channel.Writer.TryWrite(_last);   // a new listener immediately gets the current list

            var id = Guid.NewGuid();
            _subscribers[id] = channel;
            return (id, channel.Reader);
        }

        public void Unsubscribe(Guid id)
        {
            if (_subscribers.TryRemove(id, out var channel))
            {
                channel.Writer.TryComplete();
            }
        }

        public void Publish(List<PatternSuggestion> current)
        {
            _last = current;
            foreach (var channel in _subscribers.Values)
            {
                channel.Writer.TryWrite(current);
            }
        }
    }
}