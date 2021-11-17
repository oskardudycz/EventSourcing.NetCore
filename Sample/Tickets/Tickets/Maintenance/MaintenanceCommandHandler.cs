using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Marten;
using MediatR;
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

    public async Task<Unit> Handle(RebuildProjection command, CancellationToken cancellationToken)
    {
        using var daemon = documentStore.BuildProjectionDaemon();
        await daemon.RebuildProjection(command.ViewName, cancellationToken);

        return Unit.Value;
    }
}