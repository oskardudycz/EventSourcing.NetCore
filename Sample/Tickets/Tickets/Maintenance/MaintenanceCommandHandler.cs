using Core.Commands;
using Marten;
using Tickets.Maintenance.Commands;

namespace Tickets.Maintenance;

public class MaintenanceCommandHandler(IDocumentStore documentStore):
    ICommandHandler<RebuildProjection>
{
    public async Task Handle(RebuildProjection command, CancellationToken ct)
    {
        using var daemon = await documentStore.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync(command.ViewName, ct);
    }
}
