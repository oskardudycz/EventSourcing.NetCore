using System.Diagnostics;
using Core.Aggregates;
using Core.OpenTelemetry;
using Marten;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<long> Add(T aggregate, CancellationToken cancellationToken = default);
    Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
    Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
}

public class MartenRepository<T>(IDocumentSession documentSession): IMartenRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken ct) =>
        documentSession.Events.AggregateStreamAsync<T>(id, token: ct);

    public async Task<long> Add(T aggregate, CancellationToken ct = default)
    {
        var events = aggregate.DequeueUncommittedEvents();

        documentSession.Events.StartStream<Aggregate>(
            aggregate.Id,
            events
        );

        await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

        return events.Length;
    }

    public async Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken ct = default)
    {
        var events = aggregate.DequeueUncommittedEvents();

        var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;

        documentSession.Events.Append(
            aggregate.Id,
            nextVersion,
            events
        );

        await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

        return nextVersion;
    }

    public Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken ct = default) =>
        Update(aggregate, expectedVersion, ct);
}
