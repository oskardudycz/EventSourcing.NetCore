using System.Collections.Generic;
using Core.Events;

namespace Core.Aggregates
{
    public interface IEventSourcedAggregate: IAggregate
    {
        Queue<IEvent> PendingEvents { get; }
    }
}
