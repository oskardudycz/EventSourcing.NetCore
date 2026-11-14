using Core.Commands;
using Core.KurrentDB.Events;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Core.KurrentDB.Commands;

/// <summary>
/// Note: This is an example of the outbox pattern for Command Bus using KurrentDB
/// For production use mature tooling like Wolverine, MassTransit or NServiceBus
/// </summary>
public class KurrentDBAsyncCommandBus(KurrentDBClient kurrentDBClient): IAsyncCommandBus
{
    public static readonly string CommandsStreamId = "commands-external";

    public Task Schedule<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull =>
        kurrentDBClient.Append(CommandsStreamId, command, ct);
}

public static class Config
{
    public static IServiceCollection AddKurrentDBAsyncCommandBus(
        this IServiceCollection services
    ) =>
        services.AddScoped<IAsyncCommandBus, KurrentDBAsyncCommandBus>()
            .AddCommandForwarder();
}
