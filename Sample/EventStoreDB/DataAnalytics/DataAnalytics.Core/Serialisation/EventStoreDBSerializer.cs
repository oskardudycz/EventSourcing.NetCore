using System;
using System.Text;
using System.Text.Json;
using DataAnalytics.Core.Events;
using EventStore.Client;

namespace DataAnalytics.Core.Serialisation
{
    public static class EventStoreDBSerializer
    {
        public static T DeserializeData<T>(this ResolvedEvent resolvedEvent) =>
            (T)DeserializeData(resolvedEvent);

        public static object DeserializeData(this ResolvedEvent resolvedEvent) =>
            DeserializeData(resolvedEvent, EventTypeMapper.ToType(resolvedEvent.Event.EventType));

        public static object DeserializeData(this ResolvedEvent resolvedEvent, Type eventType)
        {
            // deserialize event
            return JsonSerializer.Deserialize(
                resolvedEvent.Event.Data.Span,
                eventType
            )!;
        }

        public static T DeserializeMetadata<T>(this ResolvedEvent resolvedEvent) =>
            (T)DeserializeMetadata(resolvedEvent, typeof(T));

        public static object DeserializeMetadata(this ResolvedEvent resolvedEvent, Type metadataType)
        {
            // deserialize event
            return JsonSerializer.Deserialize(
                resolvedEvent.Event.Metadata.Span,
                metadataType
            )!;
        }

        public static EventData ToJsonEventData(this object @event) =>
            new(
                Uuid.NewUuid(),
                EventTypeMapper.ToName(@event.GetType()),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { }))
            );
    }
}
