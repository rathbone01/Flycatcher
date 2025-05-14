namespace Flycatcher.Services
{
    public enum CallbackType
    {
        Channel,
        Server,
        User
    }

    public class CallbackService
    {
        // Dictionary to hold the callback functions
        private readonly Dictionary<(CallbackType type, int id), List<Func<Task>>> _callbacks = new();

        public void Subscribe(CallbackType type, int id, Func<Task> callback)
        {
            var key = (type, id);
            if (!_callbacks.TryGetValue(key, out var callbacks))
            {
                callbacks = new List<Func<Task>>();
                _callbacks[key] = callbacks;
            }
            callbacks.Add(callback);
        }

        public void Unsubscribe(CallbackType type, int id, Func<Task> callback)
        {
            var key = (type, id);
            if (_callbacks.TryGetValue(key, out var callbacks))
            {
                callbacks.Remove(callback);
                if (callbacks.Count == 0)
                {
                    _callbacks.Remove(key);
                }
            }
        }

        public async Task NotifyAsync(CallbackType type, int id)
        {
            var key = (type, id);
            if (_callbacks.TryGetValue(key, out var callbacks))
            {
                var tasks = callbacks.Select(callback => callback.Invoke());
                await Task.WhenAll(tasks);
            }
        }
    }
}
