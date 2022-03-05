using Core.Events;
using Core.EventStoreDB.Serialization;
using EventStore.Client;

namespace Core.EventStoreDB.Events;

public static class StreamEventExtensions
{
    public static EventEnvelope? ToStreamEvent(this ResolvedEvent resolvedEvent)
    {
        var eventData = resolvedEvent.Deserialize();
        var eventMetadata = resolvedEvent.DeserializeMetadata();

        if (eventData == null)
            return null;

        var metaData = new EventMetadata(
            resolvedEvent.Event.EventId.ToString(),
            resolvedEvent.Event.EventNumber.ToUInt64(),
            resolvedEvent.Event.Position.CommitPosition,
            eventMetadata
        );
        var type = typeof(EventEnvelope<>).MakeGenericType(eventData.GetType());
        return (EventEnvelope)Activator.CreateInstance(type, eventData, metaData)!;
    }
}
