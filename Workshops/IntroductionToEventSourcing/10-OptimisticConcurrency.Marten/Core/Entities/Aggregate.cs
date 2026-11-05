namespace OptimisticConcurrency.Core.Entities;

public interface IAggregate
{
    object[] DequeueUncommittedEvents();
}

public abstract class Aggregate<TEvent>: Aggregate<TEvent, Guid>;

public abstract class Aggregate<TEvent, TId>: IAggregate where TId: notnull
{
    public TId Id { get; protected set; } = default!;

    private readonly Queue<TEvent> uncommittedEvents = new();

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.Cast<object>().ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    public abstract void Apply(TEvent @event);

    protected void Enqueue(TEvent @event) =>
        uncommittedEvents.Enqueue(@event);
}
