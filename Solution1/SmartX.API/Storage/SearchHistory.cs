namespace SmartX.API.Storage
{
    // Remembers the most recent searches the user made.
    // A Queue is first-in, first-out: new terms join the back,
    // and when it is full the oldest term leaves from the front.
    public class SearchHistory
    {
        private const int MaxItems = 10;

        private readonly Queue<string> _recent = new();
        private readonly object _lock = new();

        public void Add(string term)
        {
            lock (_lock)
            {
                // Searching the same thing twice in a row is not a new entry
                if (_recent.Count > 0 && _recent.Last() == term) return;

                _recent.Enqueue(term);

                while (_recent.Count > MaxItems)
                {
                    _recent.Dequeue();   // drop the oldest
                }
            }
        }

        // Newest first, which is how a "recent searches" list reads
        public List<string> GetRecent()
        {
            lock (_lock)
            {
                return _recent.Reverse().ToList();
            }
        }
    }
}