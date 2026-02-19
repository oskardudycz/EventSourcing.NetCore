using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.OpenTelemetry.Serialization;

public class PropagationContextJsonConverter: JsonConverter
{
    private const string TraceParentPropertyName = "traceparent";
    private const string TraceStatePropertyName = "tracestate";

    public override bool CanConvert(Type objectType) =>
        objectType == typeof(PropagationContext) || objectType == typeof(PropagationContext?);

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

        return TelemetryPropagator.Extract(
            new Dictionary<string, string?>
            {
                { TraceParentPropertyName, jObject[TraceParentPropertyName]?.Value<string>() },
                { TraceStatePropertyName, jObject[TraceStatePropertyName]?.Value<string>() }
            },
            ExtractTraceContextFromEventMetadata
        );
    }

    private static IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, string?> headers, string key) =>
        headers.TryGetValue(key, out var value) && value != null
            ? [value]
            : [];
}
