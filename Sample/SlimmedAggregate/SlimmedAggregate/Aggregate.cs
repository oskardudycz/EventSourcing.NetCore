namespace SlimmedAggregate;

public abstract class Aggregate
{
    public Guid Id { get; protected set; }

    public int Version { get; protected set; }

    [NonSerialized] private readonly Queue<object> uncommittedEvents = new();

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event) =>
        uncommittedEvents.Enqueue(@event);
}
