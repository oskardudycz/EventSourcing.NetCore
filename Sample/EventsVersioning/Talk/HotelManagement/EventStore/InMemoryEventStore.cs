﻿using System.Text.Json;

namespace HotelManagement.EventStore;

public interface IEventStore
{
    ValueTask AppendToStream(Guid streamId, IEnumerable<object> newEvents, CancellationToken ct = default);
    ValueTask<TEvent[]> ReadStream<TEvent>(Guid streamId, CancellationToken ct = default) where TEvent : notnull;
}

public class InMemoryEventStore: IEventStore
{
    private readonly Dictionary<Guid, List<(string EventType, string Json)>> events = new();

    public ValueTask AppendToStream(Guid streamId, IEnumerable<object> newEvents, CancellationToken _ = default)
    {
        if (!events.ContainsKey(streamId))
            events[streamId] = [];

        var serializedEvents = newEvents.Select(e => (e.GetType().FullName!, JsonSerializer.Serialize(e)));

        events[streamId].AddRange(serializedEvents);

        return ValueTask.CompletedTask;
    }

    public ValueTask<TEvent[]> ReadStream<TEvent>(Guid streamId, CancellationToken _ = default) where TEvent : notnull
    {
        var streamEvents = events.TryGetValue(streamId, out var stream)
            ? stream
            : [];

        var deserializedEvents = streamEvents
            .Select(@event =>
                Type.GetType(@event.EventType, true) is { } clrEventType
                    ? JsonSerializer.Deserialize(@event.Json, clrEventType)
                    : null
            )
            .Where(e => e != null)
            .Cast<TEvent>().ToArray();

        return ValueTask.FromResult(deserializedEvents);
    }
}
