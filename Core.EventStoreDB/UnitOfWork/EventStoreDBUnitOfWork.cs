using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;

namespace Core.EventStoreDB.UnitOfWork;

public class EventStoreDBUnitOfWork
{
    private readonly EventStoreClient eventStore;
    private readonly List<EventData> uncommittedEvents = new();
    private readonly string streamName;
    private readonly StreamRevision expectedStreamRevision;

    private EventStoreDBUnitOfWork(EventStoreClient eventStore, string streamName,
        StreamRevision expectedStreamRevision)
    {
        this.eventStore = eventStore;
        this.streamName = streamName;
        this.expectedStreamRevision = expectedStreamRevision;
    }

    public static EventStoreDBUnitOfWork Begin(EventStoreClient eventStore, string streamName,
        StreamRevision expectedStreamRevision) =>
        new(eventStore, streamName, expectedStreamRevision);

    public Task<IWriteResult> Commit(CancellationToken cancellationToken = default)
        => eventStore.AppendToStreamAsync(
            streamName,
            expectedStreamRevision,
            uncommittedEvents.ToArray(),
            cancellationToken: cancellationToken
        );

    public void Append(params EventData[] eventData) =>
        uncommittedEvents.AddRange(eventData);
}
