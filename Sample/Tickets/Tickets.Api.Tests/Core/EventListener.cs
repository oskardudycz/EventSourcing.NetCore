using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using MediatR;

namespace Tickets.Api.Tests.Core
{
    public class EventsLog
    {
        public List<IEvent> PublishedEvents { get; } = new List<IEvent>();
    }

    public class EventListener<TEvent>: INotificationHandler<TEvent>
        where TEvent : IEvent
    {
        private readonly EventsLog eventsLog;

        public EventListener(EventsLog eventsLog)
        {
            this.eventsLog = eventsLog;
        }

        public Task Handle(TEvent @event, CancellationToken cancellationToken)
        {
            eventsLog.PublishedEvents.Add(@event);

            return Task.CompletedTask;
        }
    }
}
