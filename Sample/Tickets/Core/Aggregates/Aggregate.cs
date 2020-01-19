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

        [NonSerialized] private readonly List<IEvent> uncommittedEvents = new List<IEvent>();

        //for serialization purposes
        protected Aggregate() { }

        IEnumerable<IEvent> IAggregate.DequeueUncommittedEvents()
        {
            var dequeuedEvents = uncommittedEvents.ToList();

            uncommittedEvents.Clear();

            return dequeuedEvents;
        }

        protected void Enqueue(IEvent @event)
        {
            uncommittedEvents.Add(@event);
        }
    }
}
