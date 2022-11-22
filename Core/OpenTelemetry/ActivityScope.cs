using System.Diagnostics;

namespace Core.OpenTelemetry;

public interface IActivityScope
{
    Activity? Start(string name) =>
        Start(name, new StartActivityOptions());

    Activity? Start(string name, StartActivityOptions options);

    Task Run(
        string name,
        Func<CancellationToken, Task> run,
        CancellationToken ct
    ) => Run(name, run, new StartActivityOptions(), ct);

    Task Run(
        string name,
        Func<CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    );
}

public class ActivityScope: IActivityScope
{
    private const string CommandHandlerPrefix = "commandhandler";

    public Activity? Start(string name, StartActivityOptions options) =>
        ActivitySourceProvider.Instance
            .CreateActivity(
                $"{CommandHandlerPrefix}.{name}",
                ActivityKind.Internal,
                parentId: options.ParentId,
                idFormat: ActivityIdFormat.W3C,
                tags: options.Tags
            )?.Start();

    public async Task Run(
        string name,
        Func<CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        using var activity = Start(name, options);

        try
        {
            await run(ct);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }
}

public record StartActivityOptions
{
    public Dictionary<string, object?> Tags { get; set; } = new();

    public string? ParentId { get; set; }
}
