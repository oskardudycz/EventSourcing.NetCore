namespace Core.Events.External;

public class NulloExternalEventProducer : IExternalEventProducer
{
    public Task Publish(IEventEnvelope @event, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
