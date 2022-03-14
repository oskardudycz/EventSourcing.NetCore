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

    public static Task Add<TAggregate, TCommand>(
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

    public static Task GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Action<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
