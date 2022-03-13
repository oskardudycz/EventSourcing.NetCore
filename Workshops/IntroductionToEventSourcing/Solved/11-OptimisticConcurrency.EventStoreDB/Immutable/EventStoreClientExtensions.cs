using EventStore.Client;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

public static class EventStoreClientExtensions
{
    public static Task<TEntity> Get<TEntity>(
        this EventStoreClient eventStore,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> Add<TEntity, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> GetAndUpdate<TEntity, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, Guid> getId,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        ulong expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
