using Core.Aggregates;
using Core.Exceptions;
using Marten;

namespace Core.Marten.Aggregates;

/// <summary>
/// This code assumes that Aggregate:
/// - is event-driven but not fully event-sourced
/// - streams have string identifiers
/// - aggregate is versioned, so optimistic concurrency is applied
/// </summary>
public static class DocumentSessionExtensions
{
    public static Task Add<T>(
        this IDocumentSession documentSession,
        string id,
        T aggregate,
        CancellationToken ct
    ) where T : IAggregate
    {
        documentSession.Insert(aggregate);
        documentSession.Events.Append($"events-{id}", aggregate.DequeueUncommittedEvents());

        return documentSession.SaveChangesAsync(token: ct);
    }

    public static async Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        string id,
        Action<T> handle,
        CancellationToken ct
    ) where T : IAggregate
    {
        var aggregate = await documentSession.LoadAsync<T>(id, ct).ConfigureAwait(false);

        if (aggregate is null)
            throw AggregateNotFoundException.For<T>(id);

        handle(aggregate);

        documentSession.Update(aggregate);

        documentSession.Events.Append($"events-{id}", aggregate.DequeueUncommittedEvents());

        await documentSession.SaveChangesAsync(token: ct).ConfigureAwait(false);
    }
}
