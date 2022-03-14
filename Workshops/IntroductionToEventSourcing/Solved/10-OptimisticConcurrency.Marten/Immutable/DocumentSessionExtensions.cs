using Marten;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Immutable;

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

    public static async Task<long> Add<TEntity, TCommand>(
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

        await session.SaveChangesAsync(cancellationToken);

        return 1;
    }

    public static async Task<long> GetAndUpdate<TEntity, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        long currentVersion,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        var id = getId(command);
        var current = await session.Get<TEntity>(id, cancellationToken);

        // Marten uses the expected version AFTER event(s) is(are) appended
        var nextVersion = currentVersion + 1;

        session.Events.Append(id, nextVersion, action(command, current));

        await session.SaveChangesAsync(cancellationToken);

        return nextVersion;
    }
}
