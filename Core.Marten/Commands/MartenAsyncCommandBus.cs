using Core.Commands;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.Commands;

/// <summary>
/// Note: This is an example of the outbox pattern for Command Bus using Marten
/// For production use mature tooling like Wolverine, MassTransit or NServiceBus
/// </summary>
public class MartenAsyncCommandBus(IDocumentSession documentSession): IAsyncCommandBus
{
    public static readonly Guid CommandsStreamId = new("11111111-1111-1111-1111-111111111111");

    public Task Schedule<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull
    {
        documentSession.Events.Append(CommandsStreamId, command);
        return documentSession.SaveChangesAsync(ct);
    }
}


public static class Config
{
    public static IServiceCollection AddMartenAsyncCommandBus(
        this IServiceCollection services
    )
    {
        return services.AddScoped<IAsyncCommandBus, MartenAsyncCommandBus>()
            .AddCommandForwarder();
    }
}
