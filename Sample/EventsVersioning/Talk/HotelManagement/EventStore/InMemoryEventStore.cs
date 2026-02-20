using System.Text.Json;

namespace HotelManagement.EventStore;

public interface IEventStore
{
    ValueTask AppendToStream(
        string streamId,
        IEnumerable<object> newEvents,
        CancellationToken ct = default
    );

    ValueTask<object[]> ReadStream(
        string streamId,
        CancellationToken ct = default
    );
}

public record SerializedEvent(
    string EventType,
    string Data,
    string MetaData = ""
);

public class InMemoryEventStore(EventSerializer eventSerializer): IEventStore
{
    private readonly Dictionary<string, List<SerializedEvent>> events = new();

    public ValueTask AppendToStream(string streamId, IEnumerable<object> newEvents, CancellationToken _ = default)
    {
        if (!events.ContainsKey(streamId))
            events[streamId] = [];

        var serializedEvents = newEvents.Select(eventSerializer.Serialize);

        events[streamId].AddRange(serializedEvents);

        return ValueTask.CompletedTask;
    }

    public ValueTask<object[]> ReadStream(string streamId, CancellationToken _ = default)
    {
        var streamEvents = events.TryGetValue(streamId, out var stream)
            ? stream
            : [];

        var deserializedEvents = eventSerializer.Deserialize(streamEvents)
            .Where(e => e != null)
            .Cast<object>()
            .ToArray();

        return ValueTask.FromResult(deserializedEvents);
    }
}
