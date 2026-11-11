using System.Text.Json;

namespace OptimisticConcurrency.Core;

public record Versioned<T>(T Document, int Version);

public class Database
{
    private readonly Dictionary<string, (object Document, int Version)> storage = new();
    private readonly Lock storageLock = new();

    public ValueTask Store<T>(Guid id, T obj, CancellationToken ct) where T : class =>
        Store(id, obj, null, ct);

    public ValueTask Store<T>(Guid id, T obj, int? expectedVersion, CancellationToken _) where T : class
    {
        lock (storageLock)
        {
            var key = GetId<T>(id);
            var currentVersion = storage.TryGetValue(key, out var current) ? current.Version : 0;

            if (expectedVersion.HasValue && expectedVersion != currentVersion)
                throw new ConcurrencyException(
                    $"Expected version {expectedVersion} of {typeof(T).Name} '{id}', but it's {currentVersion}"
                );

            storage[key] = (obj, currentVersion + 1);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Delete<T>(Guid id, CancellationToken _)
    {
        lock (storageLock)
        {
            storage.Remove(GetId<T>(id));
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask<Versioned<T>?> Get<T>(Guid id, CancellationToken _) where T : class
    {
        Versioned<T>? result;

        lock (storageLock)
        {
            result = storage.TryGetValue(GetId<T>(id), out var stored)
                ?
                // Clone to simulate getting new instance on loading
                new Versioned<T>(JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)stored.Document))!, stored.Version)
                : null;
        }

        // Respond with a delay, like a real database, so handlers running in parallel read before others store
        await Task.Delay(TimeSpan.FromMilliseconds(1));

        return result;
    }

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";
}
