using System.Text.Json;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public record DataWrapper(IVersioned Data, DateTime ValidFrom);

public class Database
{
    private readonly Dictionary<Guid, List<DataWrapper>> storage = new();
    private readonly Random random = new();

    public void Store<T>(Guid id, ulong expectedVersion, T obj) where T : class, IVersioned
    {
        if(!storage.ContainsKey(id))
            storage[id] =  new List<DataWrapper>();

        var currentVersion =  storage[id].LastOrDefault()?.Data.Version ?? 0;

        if (currentVersion > expectedVersion)
            return;

        if (currentVersion != expectedVersion)
            throw new Exception("Version doesn't match");

        var validFrom = DateTime.UtcNow.AddMilliseconds(random.Next(250, 1000));

        storage[id].Add(new DataWrapper(obj, validFrom));
    }

    public T? Get<T>(Guid id, ulong expectedVersion) where T : class, IVersioned
    {
        if (!storage.TryGetValue(id, out var result))
            return null;

        var item = result.LastOrDefault(item => item.ValidFrom <= DateTime.UtcNow);

        if (item == null || item.Data.Version != expectedVersion)
            return null;

        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)item.Data));
    }
}
