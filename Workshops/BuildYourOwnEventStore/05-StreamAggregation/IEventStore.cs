using System.Collections;

namespace EventStoreBasics;

public interface IEventStore
{
    void Init();

    bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null) where TStream : notnull;

    T AggregateStream<T>(Guid streamId) where T : notnull;

    StreamState? GetStreamState(Guid streamId);

    IEnumerable GetEvents(Guid streamId);
}