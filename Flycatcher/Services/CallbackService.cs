using System;

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

        // Subscribe to a callback for a specific type (channel or server) and id
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

        // Unsubscribe from a callback for a specific type (channel or server) and id
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

        // Notify all subscribers for a specific type (channel or server) and id
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
