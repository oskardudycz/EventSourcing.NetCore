using System.Diagnostics;
using Core.Aggregates;
using Core.OpenTelemetry;
using Marten;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<long> Add(Guid id, T aggregate, CancellationToken cancellationToken = default);
    Task<long> Update(Guid id, T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
    Task<long> Delete(Guid id, T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
}

public class MartenRepository<TAggregate>(IDocumentSession documentSession): IMartenRepository<TAggregate>
    where TAggregate : class, IAggregate
{
    public Task<TAggregate?> Find(Guid id, CancellationToken ct) =>
        documentSession.Events.AggregateStreamAsync<TAggregate>(id, token: ct);

    public async Task<long> Add(Guid id, TAggregate aggregate, CancellationToken ct = default)
    {
        var events = aggregate.DequeueUncommittedEvents();

        documentSession.Events.StartStream<Aggregate>(
            id,
            events
        );

        await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

        return events.Length;
    }

    public async Task<long> Update(Guid id, TAggregate aggregate, long? expectedVersion = null, CancellationToken ct = default)
    {
        var events = aggregate.DequeueUncommittedEvents();

        var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;

        documentSession.Events.Append(
            id,
            nextVersion,
            events
        );

        await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

        return nextVersion;
    }

    public Task<long> Delete(Guid id, TAggregate aggregate, long? expectedVersion = null, CancellationToken ct = default) =>
        Update(id, aggregate, expectedVersion, ct);
}
