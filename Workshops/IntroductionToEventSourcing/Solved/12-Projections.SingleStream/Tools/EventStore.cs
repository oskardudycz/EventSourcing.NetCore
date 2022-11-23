namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public class EventStore
{
    private readonly Dictionary<Type, List<Action<EventEnvelope>>> handlers = new();
    private readonly Dictionary<Guid, List<EventEnvelope>> events = new();

    public void Register<TEvent>(Action<EventEnvelope<TEvent>> handler) where TEvent : notnull
    {
        var eventType = typeof(TEvent);

        void WrappedHandler(object @event) => handler((EventEnvelope<TEvent>)@event);

        if (handlers.ContainsKey(eventType))
            handlers[eventType].Add(WrappedHandler);
        else
            handlers.Add(eventType, new List<Action<EventEnvelope>> { WrappedHandler });
    }

    public void Append<TEvent>(Guid streamId, TEvent @event) where TEvent : notnull
    {
        if (!events.ContainsKey(streamId))
            events[streamId] = new List<EventEnvelope>();

        var eventEnvelope = new EventEnvelope<TEvent>(@event,
            EventMetadata.For(
                (ulong)events[streamId].Count + 1,
                (ulong)events.Values.Sum(s => s.Count)
            )
        );

        events[streamId].Add(eventEnvelope);

        if (!handlers.TryGetValue(eventEnvelope.Data.GetType(), out var eventHandlers)) return;

        foreach (var handle in eventHandlers)
        {
            handle(eventEnvelope);
        }
    }
}
