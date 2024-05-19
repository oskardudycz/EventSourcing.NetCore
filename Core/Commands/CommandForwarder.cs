using Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Commands;

/// <summary>
/// Note: This is an example of the outbox pattern for Command Bus using EventStoreDB
/// For production use mature tooling like Wolverine, MassTransit or NServiceBus
/// </summary>
public class CommandForwarder<T>(ICommandBus commandBus): IEventHandler<T>
    where T : notnull
{
    public async Task Handle(T command, CancellationToken ct) =>
        await commandBus.TrySend(command, ct).ConfigureAwait(false);
}

public static class CommandForwarderConfig
{
    public static IServiceCollection AddCommandForwarder(
        this IServiceCollection services
    ) =>
        services.AddTransient(typeof(IEventHandler<>), typeof(CommandForwarder<>));
}
