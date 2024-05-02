using MediatR;

namespace EventPipelines.MediatR;

public class MediatorEventRouter<T>(IEventBus eventBus): INotificationHandler<T>
    where T : INotification
{
    public async Task Handle(T notification, CancellationToken cancellationToken)
    {
        await eventBus.Publish(notification, cancellationToken);
    }
}
