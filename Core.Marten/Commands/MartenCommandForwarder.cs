using Core.Commands;
using Core.Events;

namespace Core.Marten.Commands;

/// <summary>
/// Note: This is an example of the outbox pattern for Command Bus using Marten
/// For production use mature tooling like Wolverine, MassTransit or NServiceBus
/// </summary>
public class MartenCommandForwarder<T>: IEventHandler<T> where T : notnull
{
    private readonly ICommandBus commandBus;

    public MartenCommandForwarder(ICommandBus commandBus)
    {
        this.commandBus = commandBus;
    }

    public async Task Handle(T command, CancellationToken ct)
    {
        await commandBus.TrySend(command, ct).ConfigureAwait(false);
    }
}
