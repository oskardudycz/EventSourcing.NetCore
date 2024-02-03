using EventStore.Client;

namespace CryptoShredding;

public class EventStore
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly EventConverter _eventConverter;

    public EventStore(
        EventStoreClient eventStoreClient,
        EventConverter eventConverter)
    {
        _eventStoreClient = eventStoreClient;
        _eventConverter = eventConverter;
    }

    public async Task PersistEvents(string streamName, int aggregateVersion, IEnumerable<object> eventsToPersist)
    {
        var events = eventsToPersist.ToList();
        var count = events.Count;
        if (count == 0)
        {
            return;
        }

        var expectedRevision = GetExpectedRevision(aggregateVersion, count);
        var eventsData =
            events.Select(x => _eventConverter.ToEventData(x));
        if (expectedRevision == null)
            await _eventStoreClient.AppendToStreamAsync(streamName, StreamState.NoStream, eventsData);
        else
            await _eventStoreClient.AppendToStreamAsync(streamName, expectedRevision.Value, eventsData);
    }

    public async Task<IEnumerable<object>> GetEvents(string streamName)
    {
        const int start = 0;
        const int count = 4096;
        const bool resolveLinkTos = false;
        var sliceEvents =
            _eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, start, count, resolveLinkTos: resolveLinkTos);
        var resolvedEvents = await sliceEvents.ToListAsync();
        var events =
            resolvedEvents.Select(x => _eventConverter.ToEvent(x)).Where(e => e is not null).Cast<object>();
        return events;
    }

    private StreamRevision? GetExpectedRevision(int aggregateVersion, int numberOfEvents)
    {
        var originalVersion = aggregateVersion - numberOfEvents;
        var expectedVersion = originalVersion != 0
            ? StreamRevision.FromInt64(originalVersion - 1)
            : (StreamRevision?)null;
        return expectedVersion;
    }
}
