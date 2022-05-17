using Confluent.Kafka;
using Core.Events;
using Core.Reflection;
using Core.Serialization.Newtonsoft;

namespace Core.Kafka.Events;

public static class EventEnvelopeExtensions
{
    public static IEventEnvelope? ToEventEnvelope(this ConsumeResult<string, string> message)
    {
        var eventType = TypeProvider.GetTypeFromAnyReferencingAssembly(message.Message.Key);

        if (eventType == null)
            return null;

        var eventEnvelopeType = typeof(EventEnvelope<>).MakeGenericType(eventType);

        // deserialize event
        return message.Message.Value.FromJson(eventEnvelopeType) as IEventEnvelope;
    }
}
