using System;
using Core.Events;

namespace Core.Aggregates
{
    public interface IAggregate: IAggregate<Guid>
    {
    }

    public interface IAggregate<out T>
    {
        T Id { get; }
        int Version { get; }

        void When(object @event);

        IEvent[] DequeueUncommittedEvents();
    }
}
