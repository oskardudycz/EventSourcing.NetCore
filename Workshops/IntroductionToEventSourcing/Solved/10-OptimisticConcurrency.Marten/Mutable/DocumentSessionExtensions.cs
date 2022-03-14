using Marten;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Mutable;

public static class DocumentSessionExtensions
{
    public static async Task<TAggregate> Get<TAggregate>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        var entity = await session.Events.AggregateStreamAsync<TAggregate>(id, token: cancellationToken);

        return entity ?? throw new InvalidOperationException($"Entity with id {id} was not found");
    }

    public static async Task<long> Add<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        var events = action(command).DequeueUncommittedEvents();

        // Marten uses the expected version AFTER event(s) is(are) appended
        var nextVersion = events.Length;

        session.Events.StartStream<TAggregate>(
            getId(command),
            events
        );

        await session.SaveChangesAsync(cancellationToken);

        return nextVersion;
    }

    public static async Task<long> GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Action<TCommand, TAggregate> action,
        TCommand command,
        long currentVersion,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        var id = getId(command);
        var current = await session.Get<TAggregate>(id, cancellationToken);

        action(command, current);

        var events = current.DequeueUncommittedEvents();

        // Marten uses the expected version AFTER event(s) is(are) appended
        var nextVersion = currentVersion + events.Length;

        session.Events.Append(id, nextVersion, events);

        await session.SaveChangesAsync(cancellationToken);

        return nextVersion;
    }
}
