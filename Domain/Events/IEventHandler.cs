using MediatR;

namespace Domain.Events
{
    public interface IEventHandler<in TEvent>: INotificationHandler<TEvent>
           where TEvent : IEvent
    {
    }
}
