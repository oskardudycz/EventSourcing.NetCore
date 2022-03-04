using System.Text;
using Core.Events;
using Core.Serialization.Newtonsoft;
using EventStore.Client;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Serialization;

public static class EventStoreDBSerializer
{
    private static readonly JsonSerializerSettings SerializerSettings =
        new JsonSerializerSettings().WithNonDefaultConstructorContractResolver();

    public static T? Deserialize<T>(this ResolvedEvent resolvedEvent) where T : class =>
        Deserialize(resolvedEvent) as T;

    public static object? Deserialize(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

        if (eventType == null)
            return null;

        // deserialize event
        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span),
            eventType,
            SerializerSettings
        )!;
    }

    public static EventData ToJsonEventData(this object @event, object? metadata = null) =>
        new(
            Uuid.NewUuid(),
            EventTypeMapper.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new { }))
        );
}
