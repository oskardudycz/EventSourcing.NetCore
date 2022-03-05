using Core.Events.External;
using MediatR;

namespace Core.Events.Mediator;

public interface IEventBus
{
    Task Publish(IEvent @event, CancellationToken ct);
    Task Publish(IEvent[] events, CancellationToken ct);
}

public class MediatorEventBus: IEventBus
{
    private readonly IMediator mediator;
    private readonly IExternalEventProducer externalEventProducer;

    public MediatorEventBus(
        IMediator mediator,
        IExternalEventProducer externalEventProducer
    )
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        this.externalEventProducer = externalEventProducer?? throw new ArgumentNullException(nameof(externalEventProducer));
    }

    public async Task Publish(IEvent[] events, CancellationToken ct)
    {
        foreach (var @event in events)
        {
            await Publish(@event, ct);
        }
    }

    public async Task Publish(IEvent @event, CancellationToken ct)
    {
        await mediator.Publish(@event, ct);

        if (@event is IExternalEvent externalEvent)
            await externalEventProducer.Publish(externalEvent, ct);
    }
}
