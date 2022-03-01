using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Serialization;
using Core.Repositories;
using EventStore.Client;

namespace Core.EventStoreDB.Repository;

public class EventStoreDBRepository<T>: IRepository<T> where T : class, IAggregate
{
    private readonly EventStoreClient eventStore;
    private readonly EventStoreDBExpectedStreamRevisionProvider expectedStreamRevisionProvider;
    private readonly EventStoreDBNextStreamRevisionProvider nextStreamRevisionProvider;

    public EventStoreDBRepository(
        EventStoreClient eventStore,
        EventStoreDBExpectedStreamRevisionProvider expectedStreamRevisionProvider,
        EventStoreDBNextStreamRevisionProvider nextStreamRevisionProvider
    )
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this.expectedStreamRevisionProvider = expectedStreamRevisionProvider;
        this.nextStreamRevisionProvider = nextStreamRevisionProvider;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        eventStore.AggregateStream<T>(
            id,
            cancellationToken
        );

    public async Task Add(T aggregate, CancellationToken cancellationToken)
    {
        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            StreamState.NoStream,
            GetEventsToStore(aggregate),
            cancellationToken: cancellationToken
        );
        nextStreamRevisionProvider.Set(result.NextExpectedStreamRevision);
    }

    public async Task Update(T aggregate, CancellationToken cancellationToken)
    {
        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            GetExpectedStreamRevision(),
            GetEventsToStore(aggregate),
            cancellationToken: cancellationToken
        );
        nextStreamRevisionProvider.Set(result.NextExpectedStreamRevision);
    }

    public Task Delete(T aggregate, CancellationToken cancellationToken) =>
        Update(aggregate, cancellationToken);

    private StreamRevision GetExpectedStreamRevision() =>
        expectedStreamRevisionProvider.Value ??
        throw new ArgumentNullException(nameof(expectedStreamRevisionProvider.Value),
            "Stream revision was not provided.");

    private static IEnumerable<EventData> GetEventsToStore(T aggregate)
    {
        var events = aggregate.DequeueUncommittedEvents();

        return events
            .Select(EventStoreDBSerializer.ToJsonEventData);
    }
}
