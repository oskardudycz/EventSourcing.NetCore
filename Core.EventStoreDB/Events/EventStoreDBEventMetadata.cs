using Core.Events;
using Core.Tracing.Correlation;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Events;

public class EventStoreDBEventMetadataJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(EventMetadata);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not EventMetadata(var correlationId, var causationId) || (correlationId == null && causationId == null))
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        if (correlationId != null)
        {
            writer.WritePropertyName("$correlationId");
            writer.WriteValue(correlationId);
        }
        if (causationId != null)
        {
            writer.WritePropertyName("$correlationId");
            writer.WriteValue(causationId);
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
        // var guid = serializer.Deserialize<Guid>(reader);
        // return new EventMetadata(new CorrelationId());
    }
}
