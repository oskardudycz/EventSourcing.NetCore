using System;
using Core.Events;

namespace Core.Aggregates
{
    public interface IAggregate: IAggregate<Guid>
    {
    }

    public interface IAggregate<T>
    {
        T Id { get; }
        int Version { get; }

        IEvent[] DequeueUncommittedEvents();
    }
}
