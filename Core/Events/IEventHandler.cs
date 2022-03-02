using MediatR;

namespace Core.Events;

public interface IEventHandler<in TEvent>: INotificationHandler<TEvent>
    where TEvent : IEvent
{
}
