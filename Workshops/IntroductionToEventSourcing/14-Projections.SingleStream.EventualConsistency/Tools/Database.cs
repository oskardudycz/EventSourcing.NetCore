using System.Text.Json;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public record DataWrapper(object Data, DateTime ValidFrom);

public class Database
{
    private readonly Dictionary<string, List<DataWrapper>> storage = new();
    private readonly Random random = new();

    public void Store<T>(Guid id, T obj) where T : class
    {
        if(!storage.ContainsKey(GetId<T>(id)))
            storage[GetId<T>(id)] =  new List<DataWrapper>();

        var validFrom = DateTime.UtcNow.AddMilliseconds(random.Next(50, 100));

        storage[GetId<T>(id)].Add(new DataWrapper(obj, validFrom));
    }

    public void Delete<T>(Guid id)
    {
        storage.Remove(GetId<T>(id));
    }

    public T? Get<T>(Guid id) where T : class
    {
        if (!storage.TryGetValue(GetId<T>(id), out var result))
            return null;

        var item = result.LastOrDefault(item => item.ValidFrom <= DateTime.UtcNow);

        return item != null ?
            JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)item.Data))
            : null;
    }

    private static string GetId<T>(Guid id) => $"{typeof(T).Name}-{id}";
}
