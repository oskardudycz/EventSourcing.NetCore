namespace Core.Events.External;

public interface IExternalEventProducer
{
    Task Publish(EventEnvelope @event, CancellationToken ct);
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

    public async Task Publish(object @event, CancellationToken ct)
    {
        await eventBus.Publish(@event, ct);

        if (@event is EventEnvelope { Data: IExternalEvent } eventEnvelope)
        {
            await externalEventProducer.Publish(eventEnvelope, ct);
        }
    }
}
