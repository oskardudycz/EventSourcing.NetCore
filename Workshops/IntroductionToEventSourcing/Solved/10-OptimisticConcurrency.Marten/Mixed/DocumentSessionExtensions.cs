using Marten;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Mixed;

public static class DocumentSessionExtensions
{
    public static async Task<TAggregate> Get<TAggregate>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        var entity = await session.Events.AggregateStreamAsync<TAggregate>(id, token: cancellationToken);

        return entity ?? throw new InvalidOperationException($"Entity with id {id} was not found");
    }

    public static async Task<long> Add<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        session.Events.StartStream<TAggregate>(
            getId(command),
            action(command)
        );

        await session.SaveChangesAsync(cancellationToken);
        return 1;
    }

    public static async Task<long> GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate, object> action,
        TCommand command,
        long currentVersion,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        var id = getId(command);
        var current = await session.Get<TAggregate>(id, cancellationToken);

        // Marten uses the expected version AFTER event(s) is(are) appended
        var nextVersion = currentVersion + 1;

        session.Events.Append(id, nextVersion, action(command, current));

        await session.SaveChangesAsync(cancellationToken);

        return nextVersion;
    }
}
