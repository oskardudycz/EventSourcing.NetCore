using System.Text.Json.Serialization;

namespace Idempotency.Sagas.Version1_Aggregates.Core;

public abstract class Aggregate<TEvent, TId>
    where TEvent : class
    where TId : notnull
{
    [JsonInclude] public TId Id { get; protected set; } = default!;

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

