using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Serialization;
using Core.Repositories;
using EventStore.Client;

namespace Core.EventStoreDB.Repository;

public class EventStoreDBRepository<T>: IRepository<T> where T : class, IAggregate
{
    private readonly EventStoreClient eventStore;
    private readonly IEventBus eventBus;

    public EventStoreDBRepository(
        EventStoreClient eventStoreDBClient,
        IEventBus eventBus
    )
    {
        this.eventStore = eventStoreDBClient ?? throw new ArgumentNullException(nameof(eventStoreDBClient));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken)
    {
        return eventStore.AggregateStream<T>(
            id,
            cancellationToken
        );
    }

    public Task Add(T aggregate, CancellationToken cancellationToken)
    {
        return Store(aggregate, cancellationToken);
    }

    public Task Update(T aggregate, CancellationToken cancellationToken)
    {
        return Store(aggregate, cancellationToken);
    }

    public Task Delete(T aggregate, CancellationToken cancellationToken)
    {
        return Store(aggregate, cancellationToken);
    }

    private async Task Store(T aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueUncommittedEvents();

        var eventsToStore = events
            .Select(EventStoreDBSerializer.ToJsonEventData).ToArray();

        await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            // TODO: Add proper optimistic concurrency handling
            StreamState.Any,
            eventsToStore,
            cancellationToken: cancellationToken
        );
    }
}
