namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public class Database
{
    private Dictionary<Guid, object> storage = new();

    public void Store<T>(Guid id, T obj) where T: class
    {
        storage.Add(id, obj);
    }

    public T? Get<T>(Guid id) where T: class
    {
        return storage.TryGetValue(id, out var result) ? (T)result : null;
    }
}
