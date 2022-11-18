using System.Diagnostics;

namespace Core.OpenTelemetry;

public interface IActivityScope
{
    Activity? Start(string name, params KeyValuePair<string, object?>[] tags);

    Task Run(
        string name,
        Func<CancellationToken, Task> run,
        CancellationToken ct
    ) => Run(name, run, new Dictionary<string, object?>(), ct);

    Task Run(
        string name,
        Func<CancellationToken, Task> run,
        Dictionary<string, object?> tags, CancellationToken ct
    );
}

public class ActivityScope: IActivityScope
{
    private const string CommandHandlerPrefix = "commandhandler";

    public Activity? Start(string name, params KeyValuePair<string, object?>[] tags) =>
        ActivitySourceProvider.Instance
            .CreateActivity(
                $"{CommandHandlerPrefix}.{name}",
                ActivityKind.Internal,
                parentContext: default,
                idFormat: ActivityIdFormat.W3C,
                tags: tags
            )?.Start();

    public async Task Run(
        string name,
        Func<CancellationToken, Task> run,
        Dictionary<string, object?> tags,
        CancellationToken ct
    )
    {
        using var activity = Start(name, tags.ToArray());

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
