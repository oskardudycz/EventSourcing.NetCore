using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Marten;
using MediatR;
using Tickets.Maintenance.Commands;

namespace Tickets.Reservations
{
    public class MaintenanceCommandHandler:
        IRequestHandler<RebuildProjection, Unit>
    {
        private readonly IDocumentStore documentStore;

        public MaintenanceCommandHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }

        public async Task<Unit> Handle(RebuildProjection command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            using (var daemon = documentStore.BuildProjectionDaemon())
            {
                await daemon.RebuildProjection(command.ViewName, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
