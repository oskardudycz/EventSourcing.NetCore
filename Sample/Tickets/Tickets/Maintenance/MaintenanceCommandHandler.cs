using Core.Commands;
using Marten;
using Tickets.Maintenance.Commands;

namespace Tickets.Maintenance;

public class MaintenanceCommandHandler:
    ICommandHandler<RebuildProjection>
{
    private readonly IDocumentStore documentStore;

    public MaintenanceCommandHandler(IDocumentStore documentStore)
    {
        this.documentStore = documentStore;
    }

    public async Task Handle(RebuildProjection command, CancellationToken cancellationToken)
    {
        using var daemon = await documentStore.BuildProjectionDaemonAsync();
        await daemon.RebuildProjection(command.ViewName, cancellationToken);
    }
}
