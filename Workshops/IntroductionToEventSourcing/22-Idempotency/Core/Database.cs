using System.Text.Json;

namespace Idempotency.Core;

public class Database
{
    private readonly Dictionary<string, object> storage = new();

    public ValueTask Store<T>(Guid id, T obj, CancellationToken _) where T : class
    {
        storage[GetId<T>(id)] = obj;

        return ValueTask.CompletedTask;
    }

    public ValueTask Delete<T>(Guid id, CancellationToken _)
    {
        storage.Remove(GetId<T>(id));
        return ValueTask.CompletedTask;
    }

    public ValueTask<T?> Get<T>(Guid id, CancellationToken _) where T : class =>
        ValueTask.FromResult(
            storage.TryGetValue(GetId<T>(id), out var result)
                ?
                // Clone to simulate getting new instance on loading
                JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)result))
                : null
        );

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";
}
