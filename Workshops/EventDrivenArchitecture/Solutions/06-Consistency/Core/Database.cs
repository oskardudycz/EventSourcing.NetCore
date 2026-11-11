using System.Text.Json;

namespace Consistency.Core;

public record Versioned<T>(T Document, int Version);

public record Stored<T>(Guid Id, T Document, int Version, long Position);

public class Database
{
    private readonly Dictionary<string, StoredDocument> storage;
    private readonly Lock storageLock;
    private readonly TransientFailures? failures;
    private readonly List<Write>? transactionWrites;

    private long lastPosition;

    public Database(): this(new Dictionary<string, StoredDocument>(), new Lock(), new TransientFailures("Database not available!"), null) { }

    private Database(
        Dictionary<string, StoredDocument> storage,
        Lock storageLock,
        TransientFailures? failures,
        List<Write>? transactionWrites
    )
    {
        this.storage = storage;
        this.storageLock = storageLock;
        this.failures = failures;
        this.transactionWrites = transactionWrites;
    }

    public ValueTask Store<T>(Guid id, T obj, CancellationToken ct) where T : class =>
        Store(id, obj, null, ct);

    public ValueTask Store<T>(Guid id, T obj, int? expectedVersion, CancellationToken _) where T : class
    {
        Apply(new Write(GetId<T>(id), id, typeof(T), obj, expectedVersion));

        return ValueTask.CompletedTask;
    }

    public ValueTask Delete<T>(Guid id, CancellationToken _)
    {
        Apply(new Write(GetId<T>(id), id, typeof(T), null, null));

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
                new Versioned<T>(Clone<T>(stored.Document), stored.Version)
                : null;
        }

        // Respond with a delay, like a real database, so handlers running in parallel read before others store
        await Task.Delay(TimeSpan.FromMilliseconds(1));

        return result;
    }

    // Returns documents in the order they were stored, each store gets the next position
    public ValueTask<Stored<T>[]> GetAll<T>(CancellationToken _) where T : class
    {
        lock (storageLock)
        {
            return ValueTask.FromResult(
                storage.Values
                    .Where(stored => stored.Type == typeof(T))
                    .OrderBy(stored => stored.Position)
                    .Select(stored => new Stored<T>(stored.Id, Clone<T>(stored.Document), stored.Version, stored.Position))
                    .ToArray()
            );
        }
    }

    public async ValueTask Transaction(Func<Database, ValueTask> action) =>
        await Transaction(async transaction =>
        {
            await action(transaction);
            return true;
        });

    // Changes made in the action are committed together, or not at all if the action or the commit fails
    public async ValueTask<TResult> Transaction<TResult>(Func<Database, ValueTask<TResult>> action)
    {
        var transaction = new Database(storage, storageLock, null, []);

        var result = await action(transaction);

        failures?.FailIfUnavailable();
        Commit(transaction.transactionWrites!);

        return result;
    }

    private void Apply(Write write)
    {
        if (transactionWrites != null)
        {
            transactionWrites.Add(write);
            return;
        }

        failures?.FailIfUnavailable();
        Commit([write]);
    }

    private void Commit(IReadOnlyList<Write> writes)
    {
        lock (storageLock)
        {
            foreach (var write in writes.Where(write => write.ExpectedVersion.HasValue))
            {
                var currentVersion = CurrentVersion(write.Key);

                if (write.ExpectedVersion != currentVersion)
                    throw new ConcurrencyException(
                        $"Expected version {write.ExpectedVersion} of '{write.Key}', but it's {currentVersion}"
                    );
            }

            foreach (var write in writes)
            {
                if (write.Document == null)
                    storage.Remove(write.Key);
                else
                    storage[write.Key] = new StoredDocument(
                        write.Id,
                        write.Type,
                        write.Document,
                        CurrentVersion(write.Key) + 1,
                        ++lastPosition
                    );
            }
        }
    }

    private int CurrentVersion(string key) =>
        storage.TryGetValue(key, out var stored) ? stored.Version : 0;

    private static T Clone<T>(object document) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)document))!;

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";

    private record Write(string Key, Guid Id, Type Type, object? Document, int? ExpectedVersion);

    private record StoredDocument(Guid Id, Type Type, object Document, int Version, long Position);
}
