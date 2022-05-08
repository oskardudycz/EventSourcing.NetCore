using System.Text.Json;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public record DataWrapper(object Data, DateTime ValidFrom);

public class Database
{
    private readonly Dictionary<Guid, DataWrapper> storage = new();
    private readonly Random random = new();

    public void Store<T>(Guid id, T obj) where T : class
    {
        var validFrom = DateTime.UtcNow.AddMilliseconds(random.Next(250, 1000));

        storage[id] = new DataWrapper(obj, validFrom);
    }

    public T? Get<T>(Guid id) where T : class
    {
        return storage.TryGetValue(id, out var result) && result.ValidFrom <= DateTime.UtcNow ?
            // Clone to simulate getting new instance on loading
            JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(result.Data))!
            : null;
    }
}
