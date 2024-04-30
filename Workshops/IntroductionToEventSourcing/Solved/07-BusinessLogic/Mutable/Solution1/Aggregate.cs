namespace IntroductionToEventSourcing.BusinessLogic.Mutable.Solution1;

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
