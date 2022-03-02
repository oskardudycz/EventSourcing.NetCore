using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Marten;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<long> Add(T aggregate, CancellationToken cancellationToken);
    Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
    Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
}

public class MartenRepository<T>: IMartenRepository<T> where T : class, IAggregate
{
    private readonly IDocumentSession documentSession;
    private readonly IEventBus eventBus;

    public MartenRepository(
        IDocumentSession documentSession,
        IEventBus eventBus
    )
    {
        this.documentSession = documentSession;
        this.eventBus = eventBus;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        documentSession.Events.AggregateStreamAsync<T>(id, token: cancellationToken);

    public async Task<long> Add(T aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueUncommittedEvents();

        documentSession.Events.StartStream<Aggregate>(
            aggregate.Id,
            events
        );

        await documentSession.SaveChangesAsync(cancellationToken);
        await eventBus.Publish(events, cancellationToken);

        return events.Length;
    }

    public async Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default)
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
        await eventBus.Publish(events, cancellationToken);

        return nextVersion;
    }

    public Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default) =>
        Update(aggregate, expectedVersion, cancellationToken);
}
