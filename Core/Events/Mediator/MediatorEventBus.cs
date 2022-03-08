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

    public MediatorEventBus(
        IMediator mediator
    )
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
    }
}
