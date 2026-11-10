using System.Text.Json;

namespace Consistency.Core;

public class Database
{
    private Dictionary<string, object> storage;
    private readonly TransientFailures? failures;

    public Database(): this(new Dictionary<string, object>(), new TransientFailures("Database not available!")) { }

    private Database(Dictionary<string, object> storage, TransientFailures? failures)
    {
        this.storage = storage;
        this.failures = failures;
    }

    public ValueTask Store<T>(Guid id, T obj, CancellationToken _) where T : class
    {
        failures?.FailIfUnavailable();

        storage[GetId<T>(id)] = obj;

        return ValueTask.CompletedTask;
    }

    public ValueTask Delete<T>(Guid id, CancellationToken _)
    {
        failures?.FailIfUnavailable();

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

    public ValueTask<T[]> Filter<T>(Func<T, bool> predicate, CancellationToken _) where T : class =>
        ValueTask.FromResult(
            storage.Values
                .OfType<T>()
                .Where(predicate)
                .Select(record =>
                    JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(record))!
                ).ToArray()
        );

    // Changes made in the action are committed together, or not at all if the action or the commit fails
    public async ValueTask Transaction(Func<Database, ValueTask> action)
    {
        var transaction = new Database(new Dictionary<string, object>(storage), null);

        await action(transaction);

        failures?.FailIfUnavailable();

        storage = transaction.storage;
    }

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";
}
