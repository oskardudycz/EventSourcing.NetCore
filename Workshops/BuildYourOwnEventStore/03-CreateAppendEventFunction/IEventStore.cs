using System;

namespace EventStoreBasics;

public interface IEventStore
{
    void Init();

    bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null);
}