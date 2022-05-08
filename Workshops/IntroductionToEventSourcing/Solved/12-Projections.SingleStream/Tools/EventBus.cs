namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public class EventBus
{
    private readonly Dictionary<Type, List<Action<EventEnvelope>>> handlers = new();

    public void Register<TEvent>(Action<EventEnvelope<TEvent>> handler) where TEvent : notnull
    {
        var eventType = typeof(TEvent);

        void WrappedHandler(object @event) => handler((EventEnvelope<TEvent>)@event);

        if (handlers.ContainsKey(eventType))
            handlers[eventType].Add(WrappedHandler);
        else
            handlers.Add(eventType, new List<Action<EventEnvelope>> { WrappedHandler });
    }

    public void Publish(EventEnvelope eventEnvelope)
    {
        if (!handlers.TryGetValue(eventEnvelope.Data.GetType(), out var eventHandlers)) return;

        foreach (var handle in eventHandlers)
        {
            handle(eventEnvelope);
        }
    }

    public void Publish(params EventEnvelope[] eventEnvelopes)
    {
        foreach (var @event in eventEnvelopes)
        {
            Publish(@event);
        }
    }
}
