using Core.Commands;
using Marten;

namespace Core.Marten.Commands;

public class MartenCommandBus : ICommandBus
{
    public const string CommandsStreamId = "__commands";

    private readonly IDocumentSession documentSession;

    public MartenCommandBus(IDocumentSession documentSession) =>
        this.documentSession = documentSession;

    public Task Send<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull
    {
        documentSession.Events.Append(CommandsStreamId, command);
        return documentSession.SaveChangesAsync(ct);
    }
}
