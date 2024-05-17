using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Core.Commands;

public class InMemoryCommandBus(
    IServiceProvider serviceProvider,
    CommandHandlerActivity commandHandlerActivity,
    IActivityScope activityScope,
    IAsyncPolicy retryPolicy
): ICommandBus
{
    public async Task Send<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull
    {
        var wasHandled = await TrySend(command, ct).ConfigureAwait(true);

        if (!wasHandled)
            throw new InvalidOperationException($"Unable to find handler for command '{command.GetType().Name}'");
    }

    public async Task<bool> TrySend<TCommand>(TCommand command, CancellationToken ct = default) where TCommand : notnull
    {
        var commandHandler =
            serviceProvider.GetService<ICommandHandler<TCommand>>();

        if (commandHandler == null)
            return false;

        await retryPolicy.ExecuteAsync((token) =>
                commandHandlerActivity.TrySend<TCommand>(activityScope, commandHandler.GetType().Name,
                    (_, c) => commandHandler.Handle(command, c), token),
            ct).ConfigureAwait(false);

        return true;
    }
}

public static class EventBusExtensions
{
    public static IServiceCollection AddInMemoryCommandBus(
        this IServiceCollection services,
        AsyncPolicy? asyncPolicy = null
    )
    {
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<CommandHandlerActivity>();
        services
            .AddScoped(sp =>
                new InMemoryCommandBus(
                    sp,
                    sp.GetRequiredService<CommandHandlerActivity>(),
                    sp.GetRequiredService<IActivityScope>(),
                    asyncPolicy ?? Policy.NoOpAsync()
                ))
            .TryAddScoped<ICommandBus>(sp =>
                sp.GetRequiredService<InMemoryCommandBus>()
            );

        return services;
    }
}
