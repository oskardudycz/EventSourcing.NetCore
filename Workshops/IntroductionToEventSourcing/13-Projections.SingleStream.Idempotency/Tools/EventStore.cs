namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public class EventStore
{
    private readonly Random random = new();

    private readonly Dictionary<Type, List<Action<EventEnvelope>>> handlers = new();
    private readonly Dictionary<Guid, List<EventEnvelope>> events = new();

    public void Register<TEvent>(Action<EventEnvelope<TEvent>> handler) where TEvent : notnull
    {
        var eventType = typeof(TEvent);

        void WrappedHandler(object @event) => handler((EventEnvelope<TEvent>)@event);

        if (handlers.TryGetValue(eventType, out var handler1))
            handler1.Add(WrappedHandler);
        else
            handlers.Add(eventType, [WrappedHandler]);
    }

    public void Append(EventEnvelope eventEnvelope)
    {
        if (!handlers.TryGetValue(eventEnvelope.Data.GetType(), out var eventHandlers)) return;

        foreach (var handle in eventHandlers)
        {
            var numberOfRepeatedPublish = random.Next(1, 5);

            do
            {
                handle(eventEnvelope);
            } while (--numberOfRepeatedPublish > 0);
        }
    }

    public void Append<TEvent>(Guid streamId, TEvent @event) where TEvent : notnull
    {
        if (!events.ContainsKey(streamId))
            events[streamId] = [];

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
            var numberOfRepeatedPublish = random.Next(1, 5);
            do
            {
                handle(eventEnvelope);
            } while (--numberOfRepeatedPublish > 0);
        }
    }
}
