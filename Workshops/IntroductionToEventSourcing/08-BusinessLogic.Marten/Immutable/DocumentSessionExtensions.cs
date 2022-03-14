using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

public static class DocumentSessionExtensions
{
    public static Task<TEntity> Get<TEntity>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task Add<TEntity, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task GetAndUpdate<TEntity, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
