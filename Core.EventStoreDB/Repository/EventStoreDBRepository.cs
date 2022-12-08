using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Serialization;
using Core.OpenTelemetry;
using EventStore.Client;
using OpenTelemetry.Context.Propagation;

namespace Core.EventStoreDB.Repository;

public interface IEventStoreDBRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<ulong> Add(T aggregate, CancellationToken ct = default);
    Task<ulong> Update(T aggregate, ulong? expectedRevision = null, CancellationToken ct = default);
    Task<ulong> Delete(T aggregate, ulong? expectedRevision = null, CancellationToken ct = default);
}

public class EventStoreDBRepository<T>: IEventStoreDBRepository<T> where T : class, IAggregate
{
    private readonly EventStoreClient eventStore;
    private readonly IActivityScope activityScope;

    public EventStoreDBRepository(
        EventStoreClient eventStore,
        IActivityScope activityScope
    )
    {
        this.eventStore = eventStore;
        this.activityScope = activityScope;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        eventStore.AggregateStream<T>(
            id,
            cancellationToken
        );

    public async Task<ulong> Add(T aggregate, CancellationToken ct = default)
    {
        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            StreamState.NoStream,
            GetEventsToStore(aggregate),
            cancellationToken: ct
        ).ConfigureAwait(false);

        return result.NextExpectedStreamRevision.ToUInt64();
    }

    public async Task<ulong> Update(T aggregate, ulong? expectedRevision = null, CancellationToken ct = default)
    {
        var eventsToAppend = GetEventsToStore(aggregate);
        var nextVersion = expectedRevision ?? (ulong)(aggregate.Version - eventsToAppend.Count);

        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            nextVersion,
            eventsToAppend,
            cancellationToken: ct
        ).ConfigureAwait(false);

        return result.NextExpectedStreamRevision.ToUInt64();
    }

    public Task<ulong> Delete(T aggregate, ulong? expectedRevision = null, CancellationToken ct = default) =>
        Update(aggregate, expectedRevision, ct);

    private static List<EventData> GetEventsToStore(T aggregate)
    {
        var events = aggregate.DequeueUncommittedEvents();

        return events
            .Select(@event => @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()))
            .ToList();
    }
}
