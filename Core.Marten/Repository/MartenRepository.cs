using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.Marten.OptimisticConcurrency;
using Marten;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task Add(T aggregate, CancellationToken cancellationToken);
    Task Update(T aggregate, CancellationToken cancellationToken);
    Task Delete(T aggregate, CancellationToken cancellationToken);
}

public class MartenRepository<T>: IMartenRepository<T> where T : class, IAggregate
{
    private readonly IDocumentSession documentSession;
    private readonly IEventBus eventBus;
    private readonly MartenExpectedStreamVersionProvider expectedStreamVersionProvider;
    private readonly MartenNextStreamVersionProvider nextStreamVersionProvider;

    public MartenRepository(
        IDocumentSession documentSession,
        IEventBus eventBus,
        MartenExpectedStreamVersionProvider expectedStreamVersionProvider,
        MartenNextStreamVersionProvider nextStreamVersionProvider
    )
    {
        this.documentSession = documentSession;
        this.eventBus = eventBus;
        this.expectedStreamVersionProvider = expectedStreamVersionProvider;
        this.nextStreamVersionProvider = nextStreamVersionProvider;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        documentSession.Events.AggregateStreamAsync<T>(id, token: cancellationToken);

    public async Task Add(T aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueUncommittedEvents();

        documentSession.Events.StartStream<Aggregate>(
            aggregate.Id,
            events
        );

        nextStreamVersionProvider.Set(events.Length);
        await documentSession.SaveChangesAsync(cancellationToken);
        await eventBus.Publish(events);
    }

    public async Task Update(T aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueUncommittedEvents();

        var nextVersion = GetExpectedStreamVersion() + events.Length;

        documentSession.Events.Append(
            aggregate.Id,
            nextVersion,
            events
        );

        nextStreamVersionProvider.Set(nextVersion);
        await documentSession.SaveChangesAsync(cancellationToken);
        await eventBus.Publish(events);
    }

    public Task Delete(T aggregate, CancellationToken cancellationToken) =>
        Update(aggregate, cancellationToken);


    private long GetExpectedStreamVersion() =>
        expectedStreamVersionProvider.Value ??
        throw new ArgumentNullException(nameof(expectedStreamVersionProvider.Value),
            "Stream revision was not provided.");
}
