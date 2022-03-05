using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Serialization;
using Core.Tracing;
using EventStore.Client;

namespace Core.EventStoreDB.Repository;

public interface IEventStoreDBRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);

    Task<ulong> Add(
        T aggregate,
        TraceMetadata? traceMetadata = null,
        CancellationToken ct = default
    );

    Task<ulong> Update(
        T aggregate,
        ulong? expectedRevision = null,
        TraceMetadata? traceMetadata = null,
        CancellationToken ct = default
    );

    Task<ulong> Delete(
        T aggregate,
        ulong? expectedRevision = null,
        TraceMetadata? traceMetadata = null,
        CancellationToken ct = default
    );
}

public class EventStoreDBRepository<T>: IEventStoreDBRepository<T> where T : class, IAggregate
{
    private readonly EventStoreClient eventStore;

    public EventStoreDBRepository(EventStoreClient eventStore)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        eventStore.AggregateStream<T>(
            id,
            cancellationToken
        );

    public async Task<ulong> Add(T aggregate, TraceMetadata? traceMetadata = null,
        CancellationToken ct = default)
    {
        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            StreamState.NoStream,
            GetEventsToStore(aggregate, traceMetadata),
            cancellationToken: ct
        );
        return result.NextExpectedStreamRevision;
    }

    public async Task<ulong> Update(T aggregate, ulong? expectedRevision = null, TraceMetadata? traceMetadata = null,
        CancellationToken ct = default)
    {
        var nextVersion = expectedRevision ?? (ulong)aggregate.Version;

        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            nextVersion,
            GetEventsToStore(aggregate, traceMetadata),
            cancellationToken: ct
        );
        return result.NextExpectedStreamRevision;
    }

    public Task<ulong> Delete(T aggregate, ulong? expectedRevision = null, TraceMetadata? traceMetadata = null,
        CancellationToken ct = default) =>
        Update(aggregate, expectedRevision, traceMetadata, ct);

    private static IEnumerable<EventData> GetEventsToStore(T aggregate, TraceMetadata? traceMetadata)
    {
        var events = aggregate.DequeueUncommittedEvents();

        return events
            .Select(@event => @event.ToJsonEventData(traceMetadata));
    }
}
