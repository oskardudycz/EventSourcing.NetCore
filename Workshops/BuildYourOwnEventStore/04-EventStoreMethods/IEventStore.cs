using System;
using System.Collections;

namespace EventStoreBasics;

public interface IEventStore
{
    void Init();

    bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null);

    StreamState GetStreamState(Guid streamId);

    IEnumerable GetEvents(Guid streamId);
}