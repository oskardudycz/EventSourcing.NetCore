using System.Text.Json;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public class Database
{
    private readonly Dictionary<string, object> storage = new();

    public void Store<T>(Guid id, ulong externalVersion, T obj) where T : class, IVersioned
    {
        obj.Version = externalVersion;
        storage[GetId<T>(id)] = obj;
    }

    public void Delete<T>(Guid id)
    {
        storage.Remove(GetId<T>(id));
    }

    public T? Get<T>(Guid id) where T : class, IVersioned
    {
        return storage.TryGetValue(GetId<T>(id), out var result) ?
            // Clone to simulate getting new instance on loading
            JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)result))
            : null;
    }

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";
}
