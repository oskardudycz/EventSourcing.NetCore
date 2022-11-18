using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Core.Commands;

public class CommandBus: ICommandBus
{
    private readonly IServiceProvider serviceProvider;
    private readonly AsyncPolicy retryPolicy;
    private readonly IActivityScope activitySourceCreator;

    public CommandBus(
        IServiceProvider serviceProvider,
        IActivityScope activitySourceCreator,
        AsyncPolicy retryPolicy
    )
    {
        this.serviceProvider = serviceProvider;
        this.retryPolicy = retryPolicy;
        this.activitySourceCreator = activitySourceCreator;
    }

    public Task Send<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull
    {
        var commandHandler =
            serviceProvider.GetService<ICommandHandler<TCommand>>()
            ?? throw new InvalidOperationException($"Unable to find handler for command '{command.GetType().Name}'");

        var commandName = typeof(TCommand).Name;
        var activityName = $"{commandHandler.GetType().Name}/{commandName}";

        return activitySourceCreator.Run(
            activityName,
            token => retryPolicy.ExecuteAsync(c => commandHandler.Handle(command, c), token),
            new Dictionary<string, object?> { { TelemetryTags.CommandHandling.Command, commandName } },
            ct
        );
    }
}

public static class EventBusExtensions
{
    public static IServiceCollection AddCommandBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null)
    {
        services
            .AddScoped(sp =>
                new CommandBus(
                    sp,
                    sp.GetRequiredService<IActivityScope>(),
                    asyncPolicy ?? Policy.NoOpAsync()
                ))
            .TryAddScoped<ICommandBus>(sp =>
                sp.GetRequiredService<CommandBus>()
            );

        return services;
    }
}
