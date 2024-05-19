namespace Core.Commands;

public class CommandHandlerActivity(CommandHandlerMetrics metrics)
{
    public async Task<bool> TrySend<TCommand>(
        IActivityScope activityScope,
        string commandHandlerName,
        Func<Activity?, CancellationToken, Task> run,
        CancellationToken ct
    )
    {
        var commandName = typeof(TCommand).Name;
        var activityName = $"{commandHandlerName}/{commandName}";

        var startingTimestamp = metrics.CommandHandlingStart(commandName);

        try
        {
            await activityScope.Run(
                activityName,
                run,
                new StartActivityOptions
                {
                    Tags = { { TelemetryTags.Commands.Command, commandName } }, Kind = ActivityKind.Consumer
                },
                ct
            ).ConfigureAwait(false);
        }
        finally
        {
            metrics.CommandHandlingEnd(commandName, startingTimestamp);
        }

        return true;
    }
}
