using System.Text.Json;

namespace EventsVersioning.Tests.Serializers
{
    public static class SystemTextJsonSerializer
    {
        public static string Serialize(this object @event) =>
            JsonSerializer.Serialize(@event);


        public static T? Deserialize<T>(this string @event) =>
            JsonSerializer.Deserialize<T>(@event);
    }
}
