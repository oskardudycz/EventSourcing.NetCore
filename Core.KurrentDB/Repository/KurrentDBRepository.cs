using Core.Aggregates;
using Core.Events;
using Core.KurrentDB.Events;
using Core.KurrentDB.Serialization;
using Core.OpenTelemetry;
using KurrentDB.Client;

namespace Core.KurrentDB.Repository;

public interface IKurrentDBRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<ulong> Add(Guid id, T aggregate, CancellationToken ct = default);
    Task<ulong> Update(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default);
    Task<ulong> Delete(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default);
}

public class KurrentDBRepository<T>(KurrentDBClient eventStore): IKurrentDBRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        eventStore.AggregateStream<T>(
            id,
            cancellationToken
        );

    public async Task<ulong> Add(Guid id, T aggregate, CancellationToken ct = default)
    {
        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(id),
            StreamState.NoStream,
            GetEventsToStore(aggregate),
            cancellationToken: ct
        ).ConfigureAwait(false);

        return (ulong)result.NextExpectedStreamState.ToInt64();
    }

    public async Task<ulong> Update(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default)
    {
        var eventsToAppend = GetEventsToStore(aggregate);
        var nextVersion = expectedRevision ?? (ulong)(aggregate.Version - eventsToAppend.Count);

        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(id),
            nextVersion,
            eventsToAppend,
            cancellationToken: ct
        ).ConfigureAwait(false);

        return (ulong)result.NextExpectedStreamState.ToInt64();
    }

    public Task<ulong> Delete(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default) =>
        Update(id, aggregate, expectedRevision, ct);

    private static List<EventData> GetEventsToStore(T aggregate)
    {
        var events = aggregate.DequeueUncommittedEvents();

        return events
            .Select(@event => @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()))
            .ToList();
    }
}
