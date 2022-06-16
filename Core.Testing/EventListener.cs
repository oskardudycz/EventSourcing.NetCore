using Core.Events;

namespace Core.Testing;

public class EventsLog
{
    public List<object> PublishedEvents { get; } = new();
}

public class EventListener: IEventBus
{
    private readonly IEventBus eventBus;
    private readonly EventsLog eventsLog;

    public EventListener(EventsLog eventsLog, IEventBus eventBus)
    {
        this.eventBus = eventBus;
        this.eventsLog = eventsLog;
    }

    public async Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        eventsLog.PublishedEvents.Add(eventEnvelope);
        await eventBus.Publish(eventEnvelope, ct);
    }
}

