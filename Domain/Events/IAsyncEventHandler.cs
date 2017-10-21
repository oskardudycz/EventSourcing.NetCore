using MediatR;

namespace Domain.Events
{
    public interface IAsyncEventHandler<in TEvent> : IAsyncNotificationHandler<TEvent>
           where TEvent : IEvent
    {
    }
}
