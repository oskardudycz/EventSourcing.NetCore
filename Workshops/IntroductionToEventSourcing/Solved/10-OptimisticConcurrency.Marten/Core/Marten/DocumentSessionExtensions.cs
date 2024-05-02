using Marten;
using OptimisticConcurrency.Core.Entities;
using OptimisticConcurrency.Core.Exceptions;

namespace OptimisticConcurrency.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task<int> Add<T>(this IDocumentSession documentSession, Guid id, T aggregate, CancellationToken ct)
        where T : class, IAggregate =>
        documentSession.Add<T>(id, aggregate.DequeueUncommittedEvents(), ct);

    public static async Task<int> Add<T>(this IDocumentSession documentSession, Guid id, object[] events, CancellationToken ct)
        where T : class
    {
        documentSession.Events.StartStream<T>(id, events);
        await documentSession.SaveChangesAsync(token: ct);

        return events.Length;
    }

    public static Task<int> GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        int expectedVersion,
        Action<T> handle,
        CancellationToken ct
    ) where T : class, IAggregate
        => documentSession.GetAndUpdate<T>(id, expectedVersion, state =>
        {
            handle(state);
            return state.DequeueUncommittedEvents();
        }, ct);

    public static async Task<int> GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        int expectedVersion,
        Func<T, object[]> handle,
        CancellationToken ct
    ) where T : class
    {
        var nextExpectedVersion = expectedVersion;
        await documentSession.Events.WriteToAggregate<T>(id, expectedVersion, stream =>
        {
            var aggregate = stream.Aggregate ?? throw NotFoundException.For<T>(id);
            var events = handle(aggregate);
            stream.AppendMany(events);
            nextExpectedVersion += events.Length;
        }, ct);

        return nextExpectedVersion;
    }
}
