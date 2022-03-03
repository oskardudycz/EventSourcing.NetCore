using MediatR;

namespace EventPipelines.MediatR;

public class MediatorEventRouter<T> : INotificationHandler<T> where T : INotification
{
    private readonly IEventBus eventBus;

    public MediatorEventRouter(IEventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public async Task Handle(T notification, CancellationToken cancellationToken)
    {
        await eventBus.Publish(notification, cancellationToken);
    }
}