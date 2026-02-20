using System.Text.Json;

namespace HotelManagement.EventStore;

public interface IEventSerializer
{
    SerializedEvent Serialize(object @event);
    object? Deserialize(SerializedEvent serializedEvent);
    List<object?> Deserialize(List<SerializedEvent> events);
}

public class EventSerializer(
    EventTypeMapping mapping,
    EventTransformations transformations,
    StreamTransformations? streamTransformations = null
): IEventSerializer
{
    private readonly StreamTransformations streamTransformations = streamTransformations ?? new StreamTransformations();

    public SerializedEvent Serialize(object @event) =>
        new(mapping.ToName(@event.GetType()), JsonSerializer.Serialize(@event));

    public object? Deserialize(SerializedEvent serializedEvent) =>
        transformations.TryTransform(serializedEvent.EventType, serializedEvent.Data, out var transformed)
            ? transformed
            : mapping.ToType(serializedEvent.EventType) is { } eventType
                ? JsonSerializer.Deserialize(serializedEvent.Data, eventType)
                : null;

    public List<object?> Deserialize(List<SerializedEvent> events) =>
        streamTransformations.Transform(events)
            .Select(Deserialize)
            .ToList();
}
