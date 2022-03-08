namespace Core.Events.External;

public class NulloExternalEventProducer : IExternalEventProducer
{
    public Task Publish(EventEnvelope @event, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
