using System;
using System.Collections.Generic;
using Core.Events;

namespace Core.Aggregates
{
    public abstract class EventSourcedAggregate: IEventSourcedAggregate
    {
        public Guid Id { get; protected set; }

        public Queue<IEvent> PendingEvents { get; private set; }

        protected EventSourcedAggregate()
        {
            PendingEvents = new Queue<IEvent>();
        }

        protected void Append(IEvent @event)
        {
            PendingEvents.Enqueue(@event);
        }
    }
}
