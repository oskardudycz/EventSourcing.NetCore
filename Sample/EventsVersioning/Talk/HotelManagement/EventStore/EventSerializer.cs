using System.Text.Json;

namespace HotelManagement.EventStore;

public class EventSerializer(
    EventTypeMapping mapping,
    EventTransformations transformations,
    StreamTransformations? streamTransformations = null)
{
    private readonly StreamTransformations streamTransformations = streamTransformations ?? new StreamTransformations();

    public object? Deserialize(string eventTypeName, string json) =>
        transformations.TryTransform(eventTypeName, json, out var transformed)
            ? transformed
            : JsonSerializer.Deserialize(json, mapping.ToType(eventTypeName)!);

    public List<object?> Deserialize(List<SerializedEvent> events) =>
        streamTransformations.Transform(events)
            .Select(@event => Deserialize(@event.EventType, @event.Data))
            .ToList();
}
