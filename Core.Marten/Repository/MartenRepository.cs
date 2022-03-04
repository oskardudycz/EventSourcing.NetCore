using Core.Aggregates;
using Core.Events;
using Marten;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<long> Add(T aggregate, EventMetadata? eventMetadata = null, CancellationToken cancellationToken = default);
    Task<long> Update(T aggregate, long? expectedVersion = null, EventMetadata? eventMetadata = null, CancellationToken cancellationToken = default);
    Task<long> Delete(T aggregate, long? expectedVersion = null, EventMetadata? eventMetadata = null, CancellationToken cancellationToken = default);
}

public class MartenRepository<T>: IMartenRepository<T> where T : class, IAggregate
{
    private readonly IDocumentSession documentSession;

    public MartenRepository(
        IDocumentSession documentSession
    )
    {
        this.documentSession = documentSession;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        documentSession.Events.AggregateStreamAsync<T>(id, token: cancellationToken);

    public async Task<long> Add(T aggregate, EventMetadata? eventMetadata = null, CancellationToken cancellationToken = default)
    {
        var events = aggregate.DequeueUncommittedEvents();

        documentSession.Events.StartStream<Aggregate>(
            aggregate.Id,
            events
        );

        await documentSession.SaveChangesAsync(cancellationToken);

        return events.Length;
    }

    public async Task<long> Update(T aggregate, long? expectedVersion = null, EventMetadata? eventMetadata = null, CancellationToken cancellationToken = default)
    {
        var events = aggregate.DequeueUncommittedEvents();

        var nextVersion = expectedVersion.HasValue ?
            expectedVersion.Value + events.Length
            : aggregate.Version;

        documentSession.Events.Append(
            aggregate.Id,
            aggregate.Version,
            events
        );

        await documentSession.SaveChangesAsync(cancellationToken);

        return nextVersion;
    }

    public Task<long> Delete(T aggregate, long? expectedVersion = null, EventMetadata? eventMetadata = null, CancellationToken cancellationToken = default) =>
        Update(aggregate, expectedVersion, eventMetadata, cancellationToken);
}
