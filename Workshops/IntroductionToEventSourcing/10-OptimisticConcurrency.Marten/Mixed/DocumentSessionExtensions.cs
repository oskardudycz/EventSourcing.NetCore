using Marten;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Mixed;

public static class DocumentSessionExtensions
{
    public static Task<TAggregate> Get<TAggregate>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> Add<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate, object> action,
        TCommand command,
        long version,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
