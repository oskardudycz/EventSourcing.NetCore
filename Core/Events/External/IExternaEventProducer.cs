namespace Core.Events.External;

public interface IExternalEventProducer
{
    Task Publish(IEventEnvelope @event, CancellationToken ct);
}


public class EventBusDecoratorWithExternalProducer(
    IEventBus eventBus,
    IExternalEventProducer externalEventProducer)
    : IEventBus
{
    public async Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        await eventBus.Publish(eventEnvelope, ct).ConfigureAwait(false);

        if (eventEnvelope.Data is IExternalEvent)
        {
            await externalEventProducer.Publish(eventEnvelope, ct).ConfigureAwait(false);
        }
    }
}
