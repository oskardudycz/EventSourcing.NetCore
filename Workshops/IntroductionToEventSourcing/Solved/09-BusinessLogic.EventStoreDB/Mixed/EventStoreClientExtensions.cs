using EventStore.Client;

namespace IntroductionToEventSourcing.BusinessLogic.Mixed;

public static class EventStoreClientExtensions
{
    public static Task<TAggregate> Get<TAggregate>(
        this EventStoreClient eventStore,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : IAggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> Add<TAggregate, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : IAggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> GetAndUpdate<TAggregate, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : IAggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
