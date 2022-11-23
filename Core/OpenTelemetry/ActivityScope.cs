using System.Diagnostics;

namespace Core.OpenTelemetry;

public interface IActivityScope
{
    Activity? Start(string name) =>
        Start(name, new StartActivityOptions());

    Activity? Start(string name, StartActivityOptions options);

    Task Run(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        CancellationToken ct
    ) => Run(name, run, new StartActivityOptions(), ct);

    Task Run(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    );

    Task<TResult> Run<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        CancellationToken ct
    ) => Run(name, run, new StartActivityOptions(), ct);

    Task<TResult> Run<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        StartActivityOptions options,
        CancellationToken ct
    );
}

public class ActivityScope: IActivityScope
{
    private const string CommandHandlerPrefix = "commandhandler";

    public Activity? Start(string name, StartActivityOptions options) =>
        options.Parent.HasValue
            ? ActivitySourceProvider.Instance
                .CreateActivity(
                    $"{CommandHandlerPrefix}.{name}",
                    options.Kind,
                    parentContext: options.Parent.Value,
                    idFormat: ActivityIdFormat.W3C,
                    tags: options.Tags
                )?.Start()
            : ActivitySourceProvider.Instance
                .CreateActivity(
                    $"{CommandHandlerPrefix}.{name}",
                    options.Kind,
                    parentId: options.ParentId,
                    idFormat: ActivityIdFormat.W3C,
                    tags: options.Tags
                )?.Start();

    public async Task Run(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        using var activity = Start(name, options) ?? Activity.Current;

        try
        {
            await run(activity, ct);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }

    public async Task<TResult> Run<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        using var activity = Start(name, options) ?? Activity.Current;

        try
        {
            var result = await run(activity, ct);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return result;
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

    public ActivityContext? Parent { get; set; }

    public ActivityKind Kind = ActivityKind.Internal;
}
