using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Context.Propagation;

namespace Core.OpenTelemetry.Serialization;

public class PropagationContextJsonConverter: JsonConverter
{
    private const string TraceParentPropertyName = "traceparent";
    private const string TraceStatePropertyName = "tracestate";

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PropagationContext) || objectType == typeof(PropagationContext?);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not PropagationContext propagationContext)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
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
        var jObject = JObject.Load(reader);

        var parentContext = TelemetryPropagator.Extract(
            new Dictionary<string, string?>
            {
                { TraceParentPropertyName, jObject[TraceParentPropertyName]?.Value<string>() },
                { TraceStatePropertyName, jObject[TraceStatePropertyName]?.Value<string>() }
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
            return [];
        }
    }
}
