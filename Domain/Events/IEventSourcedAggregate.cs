using System.Collections.Generic;
using Domain.Events;

namespace Domain.Aggregates
{
    public interface IEventSourcedAggregate: IAggregate
    {
        Queue<IEvent> PendingEvents { get; }
    }
}
