using System.Diagnostics;

namespace Core.OpenTelemetry;

public static class ActivitySourceProvider
{
    public const string DefaultSourceName = "eventsourcing.net";
    public static readonly ActivitySource Instance = new(DefaultSourceName, "v1");
}
