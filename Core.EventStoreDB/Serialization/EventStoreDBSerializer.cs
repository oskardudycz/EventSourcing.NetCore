using System.Text.Json;
using Core.Events;
using Core.EventStoreDB.Events;
using EventStore.Client;
using OpenTelemetry.Context.Propagation;

namespace Core.EventStoreDB.Serialization;

public static class EventStoreDBSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new EventStoreDBEventMetadataJsonConverter() }
    };

    public static T? Deserialize<T>(this ResolvedEvent resolvedEvent) where T : class =>
        Deserialize(resolvedEvent) as T;

    public static object? Deserialize(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.Instance.ToType(resolvedEvent.Event.EventType);

        if (eventType == null)
            return null;

        // deserialize event
        return JsonSerializer.Deserialize(
            resolvedEvent.Event.Data.Span,
            eventType,
            SerializerOptions
        )!;
    }

    public static PropagationContext? DeserializePropagationContext(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.Instance.ToType(resolvedEvent.Event.EventType);

        if (eventType == null)
            return null;

        // deserialize event
        return JsonSerializer.Deserialize<PropagationContext>(
            resolvedEvent.Event.Metadata.Span,
            SerializerOptions
        );
    }

    public static EventData ToJsonEventData(this object @event, object? metadata = null) =>
        new(
            Uuid.NewUuid(),
            EventTypeMapper.Instance.ToName(@event.GetType()),
            JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), SerializerOptions),
            JsonSerializer.SerializeToUtf8Bytes(metadata ?? new { }, SerializerOptions)
        );
}
