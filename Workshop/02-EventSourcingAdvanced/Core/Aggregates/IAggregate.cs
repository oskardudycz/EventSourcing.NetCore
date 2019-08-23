using System;
using System.Collections.Generic;
using Core.Events;

namespace Core.Aggregates
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }

        IEnumerable<IEvent> DequeueUncommittedEvents();
    }
}
