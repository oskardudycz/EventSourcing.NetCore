namespace Core.Events.External;

public class NulloExternalEventProducer : IExternalEventProducer
{
    public Task Publish(IExternalEvent @event, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
