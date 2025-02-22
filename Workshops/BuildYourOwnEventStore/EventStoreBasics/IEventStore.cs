using System.Collections;

namespace EventStoreBasics;

public interface IEventStore
{
    Task Init(CancellationToken ct = default);

    void AddSnapshot(ISnapshot snapshot);

    void AddProjection(IProjection projection);

    Task<bool> AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null, CancellationToken ct = default) where TStream: notnull;

    Task<T?> AggregateStream<T>(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null, CancellationToken ct = default) where T: notnull;

    Task<StreamState?> GetStreamState(Guid streamId, CancellationToken ct = default);

    Task<IEnumerable> GetEvents(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null, CancellationToken ct = default);

    Task<bool> Store<TStream>(TStream aggregate, CancellationToken ct = default) where TStream : IAggregate;
}
