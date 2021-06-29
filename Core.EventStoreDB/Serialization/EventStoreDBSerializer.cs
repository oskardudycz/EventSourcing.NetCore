using System.Text;
using Core.Events;
using Core.Reflection;
using EventStore.Client;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Serialization
{
    public static class EventStoreDBSerializer
    {
        public static T Deserialize<T>(this ResolvedEvent resolvedEvent) => (T)Deserialize(resolvedEvent);

        public static object Deserialize(this ResolvedEvent resolvedEvent)
        {
            // get type
            // TODO: Add type mapping
            var eventType = TypeProvider.GetTypeFromAnyReferencingAssembly(resolvedEvent.Event.EventType);

            // deserialize event
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), eventType!)!;
        }

        public static EventData ToJsonEventData(this IEvent @event) =>
            new(
                Uuid.NewUuid(),
                // TODO: Add type mapping
                @event.GetType().FullName!,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }))
            );
    }
}
