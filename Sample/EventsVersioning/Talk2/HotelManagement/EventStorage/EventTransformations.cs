using System.Text.Json;

namespace HotelManagement.EventStorage;

public class EventTransformations
{
    private readonly Dictionary<string, Func<string, object>> jsonTransformations = new();

    public bool TryTransform(string eventTypeName, string json, out object? result)
    {
        if (!jsonTransformations.TryGetValue(eventTypeName, out var transformJson))
        {
            result = null;
            return false;
        }

        result = transformJson(json);
        return true;
    }



    public EventTransformations Register<TEvent>(string eventTypeName, Func<JsonDocument, TEvent> transformJson)
        where TEvent : notnull
    {
        jsonTransformations.Add(
            eventTypeName,
            json => transformJson(JsonDocument.Parse(json))
        );
        return this;
    }

    public EventTransformations Register<TOldEvent, TEvent>(string eventTypeName,
        Func<TOldEvent, TEvent> transformEvent)
        where TOldEvent : notnull
        where TEvent : notnull
    {
        jsonTransformations.Add(
            eventTypeName,
            json => transformEvent(JsonSerializer.Deserialize<TOldEvent>(json)!)
        );
        return this;
    }
}
