using Core.Events;
using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Events;

public class EventStoreDBEventMetadataJsonConverter : JsonConverter
{
    private const string CorrelationIdPropertyName = "$correlationId";
    private const string CausationIdPropertyName = "$causationId";

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TraceMetadata);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not TraceMetadata(var correlationId, var causationId) || (correlationId == null && causationId == null))
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        if (correlationId != null)
        {
            writer.WritePropertyName(CorrelationIdPropertyName);
            writer.WriteValue(correlationId.Value);
        }
        if (causationId != null)
        {
            writer.WritePropertyName(CausationIdPropertyName);
            writer.WriteValue(causationId.Value);
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        CorrelationId? correlationId = null;
        CausationId? causationId = null;
        do
        {

            if (!reader.Read()) break;

            if (reader.Value is not string propertyName)
                continue;

            var propertyValue = reader.ReadAsString();

            if(propertyValue == null)
                continue;

            switch (propertyName)
            {
                case CorrelationIdPropertyName:
                    correlationId = new CorrelationId(propertyValue);
                    break;
                case CausationIdPropertyName:
                    causationId = new CausationId(propertyValue);
                    break;
            }
        } while (true);


        return new TraceMetadata(correlationId, causationId);
    }
}
