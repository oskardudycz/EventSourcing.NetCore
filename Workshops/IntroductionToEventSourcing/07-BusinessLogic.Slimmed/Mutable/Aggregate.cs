namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Mutable;

public abstract class Aggregate
{
    public Guid Id { get; protected set; } = default!;

    private readonly Queue<object> uncommittedEvents = new();

    public virtual void Evolve(object @event) { }

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event)
    {
        uncommittedEvents.Enqueue(@event);
    }
}
