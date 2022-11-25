using Core.OpenTelemetry;
using Newtonsoft.Json;
using OpenTelemetry.Context.Propagation;

namespace Core.EventStoreDB.Events;

public class EventStoreDBEventMetadataJsonConverter: JsonConverter
{
    private const string CorrelationIdPropertyName = "$correlationId";
    private const string CausationIdPropertyName = "$causationId";

    private const string TraceParentPropertyName = "traceparent";
    private const string TraceStatePropertyName = "tracestate";

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PropagationContext);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not PropagationContext propagationContext)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName(CorrelationIdPropertyName);
        writer.WriteValue(propagationContext.ActivityContext.TraceId.ToHexString());

        writer.WritePropertyName(CausationIdPropertyName);
        writer.WriteValue(propagationContext.ActivityContext.SpanId.ToHexString());

        propagationContext.Inject(writer, SerializePropagationContext);
    }

    private static void SerializePropagationContext(JsonWriter writer, string key, string value)
    {
        writer.WritePropertyName(key);
        writer.WriteValue(value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        string? traceParent = null;
        string? traceState = null;
        do
        {
            if (!reader.Read()) break;

            if (reader.Value is not string propertyName)
                continue;

            var propertyValue = reader.ReadAsString();

            if (propertyValue == null)
                continue;

            switch (propertyName)
            {
                case TraceParentPropertyName:
                    traceParent = propertyValue;
                    break;
                case TraceStatePropertyName:
                    traceState = propertyValue;
                    break;
            }
        } while (true);

        var parentContext = TelemetryPropagator.Extract(
            new Dictionary<string, string?>
            {
                { TraceParentPropertyName, traceParent }, { TraceStatePropertyName, traceState }
            },
            ExtractTraceContextFromEventMetadata
        );

        return parentContext;
    }

    private static IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, string?> headers,
        string key)
    {
        try
        {
            return headers.TryGetValue(key, out var value) && value != null
                ? new[] { value }
                : Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract trace context: {ex}");
            return Enumerable.Empty<string>();
        }
    }
}
