using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Core.Commands;

public class InMemoryCommandBus: ICommandBus
{
    private readonly IServiceProvider serviceProvider;
    private readonly AsyncPolicy retryPolicy;
    private readonly IActivityScope activityScope;

    public InMemoryCommandBus(
        IServiceProvider serviceProvider,
        IActivityScope activityScope,
        AsyncPolicy retryPolicy
    )
    {
        this.serviceProvider = serviceProvider;
        this.retryPolicy = retryPolicy;
        this.activityScope = activityScope;
    }

    public async Task Send<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull
    {
        var wasHandled = await TrySend(command, ct);

        if(!wasHandled)
            throw new InvalidOperationException($"Unable to find handler for command '{command.GetType().Name}'");
    }

    public async Task<bool> TrySend<TCommand>(TCommand command, CancellationToken ct = default) where TCommand : notnull
    {
        var commandHandler =
            serviceProvider.GetService<ICommandHandler<TCommand>>();

        if (commandHandler == null)
            return false;

        var commandName = typeof(TCommand).Name;
        var activityName = $"{commandHandler.GetType().Name}/{commandName}";

        await activityScope.Run(
            activityName,
            (_, token) => retryPolicy.ExecuteAsync(c => commandHandler.Handle(command, c), token),
            new StartActivityOptions { Tags = { { TelemetryTags.CommandHandling.Command, commandName } } },
            ct
        );

        return true;
    }
}

public static class EventBusExtensions
{
    public static IServiceCollection AddInMemoryCommandBus(this IServiceCollection services,
        AsyncPolicy? asyncPolicy = null)
    {
        services
            .AddScoped(sp =>
                new InMemoryCommandBus(
                    sp,
                    sp.GetRequiredService<IActivityScope>(),
                    asyncPolicy ?? Policy.NoOpAsync()
                ))
            .TryAddScoped<ICommandBus>(sp =>
                sp.GetRequiredService<InMemoryCommandBus>()
            );

        return services;
    }
}
