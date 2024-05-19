namespace ApplicationLogic.Marten.Core.Entities;

public interface IAggregate
{
    public void Apply(object @event);

    object[] DequeueUncommittedEvents();
}

public abstract class Aggregate<TEvent>: IAggregate
{
    public Guid Id { get; protected set; } = default!;

    private readonly Queue<object> uncommittedEvents = new();

    protected virtual void Apply(TEvent @event) { }

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event) =>
        uncommittedEvents.Enqueue(@event);

    public void Apply(object @event)
    {
        if(@event is not TEvent typed)
            return;

        Apply(typed);
    }
}
