namespace Core.OpenTelemetry;

public static class TelemetryTags
{
    public static class CommandHandling
    {
        public const string Command = $"{ActivitySourceProvider.DefaultSourceName}.command";
    }

    public static class EventHandling
    {
        public const string Event = $"{ActivitySourceProvider.DefaultSourceName}.event";
    }
}
