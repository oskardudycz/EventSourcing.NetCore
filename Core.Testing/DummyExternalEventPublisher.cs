using Core.Events;
using Core.Events.External;

namespace Core.Testing;

public class DummyExternalEventProducer: IExternalEventProducer
{
    public IList<object> PublishedEvents { get; } = new List<object>();

    public Task Publish(IEventEnvelope @event, CancellationToken ct)
    {
        PublishedEvents.Add(@event.Data);

        return Task.CompletedTask;
    }
}
