namespace Core.Events.External;

public interface IExternalEventProducer
{
    Task Publish(IEventEnvelope @event, CancellationToken ct);
}


public class EventBusDecoratorWithExternalProducer: IEventBus
{
    private readonly IEventBus eventBus;
    private readonly IExternalEventProducer externalEventProducer;

    public EventBusDecoratorWithExternalProducer(
        IEventBus eventBus,
        IExternalEventProducer externalEventProducer
    )
    {
        this.eventBus = eventBus;
        this.externalEventProducer = externalEventProducer;
    }

    public async Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        await eventBus.Publish(eventEnvelope, ct);

        if (eventEnvelope.Data is IExternalEvent)
        {
            await externalEventProducer.Publish(eventEnvelope, ct);
        }
    }
}
