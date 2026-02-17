using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{
    public class CallbackService
    {
        // Dictionary to hold the callback functions
        private readonly Dictionary<(CallbackType type, Guid id), List<Func<Task>>> _callbacks = new();

        public void Subscribe(CallbackType type, Guid id, Func<Task> callback)
        {
            var key = (type, id);

            // If the key does not exist, create a new list for callbacks
            if (!_callbacks.TryGetValue(key, out var callbacks))
            {
                callbacks = new List<Func<Task>>();
                _callbacks[key] = callbacks;
            }

            // Add the callback to the list
            callbacks.Add(callback);
        }

        public void Unsubscribe(CallbackType type, Guid id, Func<Task> callback)
        {
            var key = (type, id);
            if (_callbacks.TryGetValue(key, out var callbacks))
            {
                callbacks.Remove(callback);

                // If there are no more callbacks for this key, remove it from the dictionary
                if (callbacks.Count == 0)
                {
                    _callbacks.Remove(key);
                }
            }
        }

        public async Task NotifyAsync(CallbackType type, Guid id)
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
