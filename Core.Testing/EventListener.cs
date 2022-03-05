using Core.Events;

namespace Core.Testing;

public class EventsLog
{
    public List<IEvent> PublishedEvents { get; } = new();
}

public class EventListener<TEvent>: IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly EventsLog eventsLog;

    public EventListener(EventsLog eventsLog)
    {
        this.eventsLog = eventsLog;
    }

    public Task Handle(TEvent @event, CancellationToken cancellationToken)
    {
        eventsLog.PublishedEvents.Add(@event);

        return Task.CompletedTask;
    }
}
