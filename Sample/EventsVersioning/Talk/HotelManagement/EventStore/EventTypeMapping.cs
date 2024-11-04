namespace HotelManagement.EventStore;

public class EventTypeMapping
{
    private readonly Dictionary<string, Type> mappings = new();

    public EventTypeMapping Register<TEvent>(params string[] typeNames)
    {
        var eventType = typeof(TEvent);

        foreach (var typeName in typeNames)
        {
            mappings.Add(typeName, eventType);
        }

        return this;
    }

    public Type Map(string eventType) => mappings[eventType];
}
