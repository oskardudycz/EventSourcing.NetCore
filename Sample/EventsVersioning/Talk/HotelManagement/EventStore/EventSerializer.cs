using System.Text.Json;

namespace HotelManagement.EventStore;

public class EventSerializer(EventTypeMapping mapping, EventTransformations transformations)
{
    public object? Deserialize(string eventTypeName, string json) =>
        transformations.TryTransform(eventTypeName, json, out var transformed)
            ? transformed
            : JsonSerializer.Deserialize(json, mapping.ToType(eventTypeName)!);
}
