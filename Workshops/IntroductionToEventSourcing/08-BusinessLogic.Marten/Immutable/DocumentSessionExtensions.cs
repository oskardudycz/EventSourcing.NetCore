using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

public static class DocumentSessionExtensions
{
    public static Task<TEntity> Get<TEntity>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task<long> Add<TEntity, TCommand>(
        this IDocumentSession session,
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
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
