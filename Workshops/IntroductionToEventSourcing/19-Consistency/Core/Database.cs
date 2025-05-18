using System.Text.Json;

namespace Consistency.Core;

public class Database
{
    private Dictionary<string, object> storage = new();

    public ValueTask Store<T>(Guid id, T obj, CancellationToken _) where T : class
    {
        Monitor.Enter(storage);
        try
        {
            if (DateTimeOffset.Now.Ticks % 5 == 0)
                throw new TimeoutException("Database not available!");

            storage[GetId<T>(id)] = obj;

            return ValueTask.CompletedTask;
        }
        finally
        {
            Monitor.Exit(storage);
        }
    }

    public ValueTask Delete<T>(Guid id, CancellationToken _)
    {
        Monitor.Enter(storage);
        try
        {
            if (DateTimeOffset.Now.Ticks % 5 == 0)
                throw new TimeoutException("Database not available!");

            storage.Remove(GetId<T>(id));

            return ValueTask.CompletedTask;
        }
        finally
        {
            Monitor.Exit(storage);
        }
    }

    public ValueTask<T?> Get<T>(Guid id, CancellationToken _) where T : class =>
        ValueTask.FromResult(
            storage.TryGetValue(GetId<T>(id), out var result)
                ?
                // Clone to simulate getting new instance on loading
                JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)result))
                : null
        );

    public ValueTask<T[]> Filter<T>(Func<T, bool> predicate, CancellationToken _) where T : class =>
        ValueTask.FromResult(
            storage.Values
                .OfType<T>()
                .Where(predicate)
                .Select(record =>
                    JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(record))!
                ).ToArray()
        );

    public async ValueTask Transaction(Func<Database, ValueTask> action)
    {
        Monitor.Enter(storage);
        try
        {
            var serialisedDatabase = new Database();

            await action(serialisedDatabase);
        }
        finally
        {
            Monitor.Exit(storage);
        }
    }

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";
}
