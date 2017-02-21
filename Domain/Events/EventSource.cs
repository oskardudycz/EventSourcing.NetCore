using Domain.Events;
using System;
using System.Collections.Generic;

namespace Domain.Aggregates
{
    public abstract class EventSource : IAggregate
    {
        public Guid Id { get; protected set; }

        public Queue<IEvent> PendingEvents { get; private set; }
        
        protected EventSource()
        {
            PendingEvents = new Queue<IEvent>();
        }

        protected void Append(IEvent @event)
        {
            PendingEvents.Enqueue(@event);
        }
    }
}
