using Core.Commands;
using Core.EventStoreDB.Events;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Commands;

/// <summary>
/// Note: This is an example of the outbox pattern for Command Bus using EventStoreDB
/// For production use mature tooling like Wolverine, MassTransit or NServiceBus
/// </summary>
public class EventStoreDBAsyncCommandBus(EventStoreClient eventStoreClient): IAsyncCommandBus
{
    public static readonly string CommandsStreamId = "commands-external";

    public Task Schedule<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull =>
        eventStoreClient.Append(CommandsStreamId, command, ct);
}

public static class Config
{
    public static IServiceCollection AddEventStoreDBAsyncCommandBus(
        this IServiceCollection services
    ) =>
        services.AddScoped<IAsyncCommandBus, EventStoreDBAsyncCommandBus>()
            .AddCommandForwarder();
}
