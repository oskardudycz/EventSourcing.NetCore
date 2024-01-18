using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution1;

public static class DocumentSessionExtensions
{
    public static async Task<TEntity> Get<TEntity>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        var entity = await session.Events.AggregateStreamAsync<TEntity>(id, token: cancellationToken);

        return entity ?? throw new InvalidOperationException($"Entity with id {id} was not found");
    }

    public static Task Add<TEntity, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        session.Events.StartStream<TEntity>(
            getId(command),
            action(command)
        );

        return session.SaveChangesAsync(cancellationToken);
    }

    public static async Task GetAndUpdate<TEntity, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        var id = getId(command);
        var current = await session.Get<TEntity>(id, cancellationToken);

        session.Events.Append(id, action(command, current));

        await session.SaveChangesAsync(cancellationToken);
    }
}
