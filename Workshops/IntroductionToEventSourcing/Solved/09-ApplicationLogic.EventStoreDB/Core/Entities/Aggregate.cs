namespace ApplicationLogic.EventStoreDB.Core.Entities;

public interface IAggregate
{
    public void Evolve(object @event);

    object[] DequeueUncommittedEvents();
}

public abstract class Aggregate<TEvent>: IAggregate
{
    public Guid Id { get; protected set; }

    private readonly Queue<TEvent> uncommittedEvents = new();

    public abstract void Evolve(TEvent @event);

    public TEvent[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(TEvent @event)
    {
        uncommittedEvents.Enqueue(@event);
    }

    public void Evolve(object @event)
    {
        if(@event is not TEvent typed)
            return;

        Evolve(typed);
    }

    object[] IAggregate.DequeueUncommittedEvents() =>
        DequeueUncommittedEvents().Cast<object>().ToArray();
}
