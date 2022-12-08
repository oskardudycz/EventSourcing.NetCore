using EventStore.Client;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

public static class EventStoreClientExtensions
{
    public static Task<TEntity> Get<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity, object, TEntity> when,
        TEntity empty,
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task Add<TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, string> getStreamName,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task GetAndUpdate<TEntity, TCommand>(
        this EventStoreClient eventStore,
        Func<TEntity, object, TEntity> when,
        TEntity empty,
        Func<TCommand, string> getStreamName,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
