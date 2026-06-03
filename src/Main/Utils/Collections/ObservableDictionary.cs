using Monod.Shared.Exceptions;

namespace Monod.Utils.Collections;

public sealed class ObservableDict<TKey, TValue> where TKey : notnull where TValue : class
{
    private readonly Lock _lock = new();
    private readonly Dictionary<TKey, object> Dict = [];
    public IEnumerable<object> Values => Dict.Values;

    public Task<TValue> Request(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        lock (_lock)
        {

            if (Dict.TryGetValue(key, out object? obj))
            {
                if (obj is TValue value)
                    return Task.FromResult(value);
                if (obj is TaskCompletionSource<TValue> listener)
                    return listener.Task;
                Guard.InvalidOperationException($"obj is not any valid type: {obj}");
                return null!;
            }
            else
            {
                TaskCompletionSource<TValue> listener = new();
                Dict.Add(key, listener);
                return listener.Task;
            }
        }
    }

    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        lock (_lock)
        {
            if (Dict.TryGetValue(key, out object? obj) && obj is TaskCompletionSource<TValue> listener)
                listener.SetResult(value);

            Dict[key] = value;
        }
    }

    public void InvalidCurrentRequests()
    {
        lock (_lock)
        {
            foreach (object obj in Dict.Values)
            {
                if (obj is TaskCompletionSource<TValue?> listener)
                    listener.SetResult(null);
            }
        }
    }
}
