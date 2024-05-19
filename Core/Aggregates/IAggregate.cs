namespace Core.Aggregates;

public interface IAggregate: IProjection
{
    int Version { get; }

    object[] DequeueUncommittedEvents();
}

public interface IAggregate<in TEvent>: IAggregate, IProjection<TEvent> where TEvent : class;
