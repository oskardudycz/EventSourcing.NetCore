using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;

namespace Core.Aggregates
{
    public abstract class Aggregate: IAggregate
    {
        public Guid Id { get; protected set; }

        public int Version { get; protected set; }

        [NonSerialized] private readonly Queue<IEvent> uncommittedEvents = new Queue<IEvent>();

        //for serialization purposes
        protected Aggregate() { }

        public IEvent[] DequeueUncommittedEvents()
        {
            var dequeuedEvents = uncommittedEvents.ToArray();

            uncommittedEvents.Clear();

            return dequeuedEvents;
        }

        protected void Enqueue(IEvent @event)
        {
            uncommittedEvents.Enqueue(@event);
        }
    }
}
