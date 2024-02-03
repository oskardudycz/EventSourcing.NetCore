using CryptoShredding.Contracts;
using CryptoShredding.Serialization;
using EventStore.Client;

namespace CryptoShredding;

public class EventConverter
{
    private readonly JsonSerializer _jsonSerializer;

    public EventConverter(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public IEvent? ToEvent(ResolvedEvent resolvedEvent)
    {
        var data = resolvedEvent.Event.Data;
        var metadata = resolvedEvent.Event.Metadata;
        var eventName = resolvedEvent.Event.EventType;
        var persistableEvent = _jsonSerializer.Deserialize(data, metadata, eventName);
        return persistableEvent;
    }

    public EventData ToEventData(IEvent @event)
    {
        var eventTypeName = @event.GetType().Name;
        var id = Uuid.NewUuid();
        var serializedEvent = _jsonSerializer.Serialize(@event);
        var contentType = serializedEvent.IsJson ? "application/json" : "application/octet-stream";
        var data = serializedEvent.Data;
        var metadata = serializedEvent.MetaData;
        var eventData = new EventData(id, eventTypeName,data, metadata, contentType);
        return eventData;
    }
}
