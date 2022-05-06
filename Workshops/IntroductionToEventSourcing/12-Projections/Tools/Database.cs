using System.Text.Json;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public class Database
{
    private readonly Dictionary<Guid, object> storage = new();

    public void Store<T>(Guid id, T obj) where T: class
    {
        storage[id] = obj;
    }

    public T? Get<T>(Guid id) where T: class
    {
        return storage.TryGetValue(id, out var result) ?
            // Clone to simulate getting new instance on loading
            JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(result))!
            : null;
    }
}
