using System.Text.Json.Serialization;

namespace EventStoreBasics;

public class Aggregate: IAggregate
{
    [JsonInclude]
    public Guid Id { get; protected set; }

    [JsonInclude]
    public int Version { get; protected set; }

    [JsonIgnore]
    private readonly List<object> uncommittedEvents = [];

    //for serialization purposes
    protected Aggregate() { }

    IEnumerable<object> IAggregate.DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToList();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event)
    {
        Version++;
        uncommittedEvents.Add(@event);
    }
}