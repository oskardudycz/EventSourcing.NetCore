using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.OpenTelemetry.Serialization;

public class PropagationContextJsonConverter: JsonConverter<PropagationContext>
{
    public override void Write(Utf8JsonWriter writer, PropagationContext value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        value.Inject(writer, static (json, key, header) => json.WriteString(key, header));
        writer.WriteEndObject();
    }

    public override PropagationContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var headers = JsonSerializer.Deserialize<Dictionary<string, string?>>(ref reader, options)
                      ?? throw new JsonException("Expected propagation context headers.");

        return TelemetryPropagator.Extract(headers, static (values, key) =>
            values.TryGetValue(key, out var value) && value != null ? [value] : []);
    }
}
