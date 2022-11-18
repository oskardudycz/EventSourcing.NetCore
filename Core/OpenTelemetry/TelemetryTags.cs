namespace Core.OpenTelemetry;

public static class TelemetryTags
{
    public static class CommandHandling
    {
        public const string Command = $"{ActivitySourceProvider.DefaultSourceName}.command";
    }
}
