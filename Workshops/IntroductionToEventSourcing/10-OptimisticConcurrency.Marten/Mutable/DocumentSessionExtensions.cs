using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Mutable;

public static class DocumentSessionExtensions
{
    public static Task<TAggregate> Get<TAggregate>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> Add<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Action<TCommand, TAggregate> action,
        TCommand command,
        long expectedVersion,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
