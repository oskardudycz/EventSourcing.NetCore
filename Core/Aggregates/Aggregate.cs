namespace Core.Aggregates;

public abstract class Aggregate: Aggregate<object, Guid>;

public abstract class Aggregate<TEvent>: Aggregate<TEvent, Guid> where TEvent : class;

public abstract class Aggregate<TEvent, TId>: IAggregate<TEvent>
    where TEvent : class
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public int Version { get; protected set; }

    [NonSerialized] private readonly Queue<TEvent> uncommittedEvents = new();

    public virtual void Apply(TEvent @event) { }

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.Cast<object>().ToArray();;

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(TEvent @event)
    {
        uncommittedEvents.Enqueue(@event);
        Apply(@event);
    }
}
