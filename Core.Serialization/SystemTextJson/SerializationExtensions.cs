using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Serialization.SystemTextJson;

public static class SerializationExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static JsonSerializerOptions WithDefaults(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static T FromJson<T>(this string json, params JsonConverter[] converters) =>
        JsonSerializer.Deserialize<T>(json, WithConverters(converters))!;

    public static object? FromJson(this string json, Type type, params JsonConverter[] converters) =>
        JsonSerializer.Deserialize(json, type, WithConverters(converters));

    public static string ToJson(this object obj, params JsonConverter[] converters) =>
        JsonSerializer.Serialize(obj, obj.GetType(), WithConverters(converters));

    public static StringContent ToJsonStringContent(this object obj) =>
        new(obj.ToJson(), Encoding.UTF8, "application/json");

    private static JsonSerializerOptions WithConverters(JsonConverter[] converters)
    {
        if (converters.Length == 0)
            return DefaultOptions;

        var options = new JsonSerializerOptions(DefaultOptions);
        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }
}
