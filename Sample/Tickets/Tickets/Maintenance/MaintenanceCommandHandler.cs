using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Baseline.Dates;
using Marten;
using Marten.Events.Projections.Async;
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

            var projectionType =  Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t=>t.Name == command.ProjectionName);

            using (var daemon = documentStore.BuildProjectionDaemon(
                new [] { projectionType },
                settings: new DaemonSettings
                {
                    LeadingEdgeBuffer = 0.Seconds()
                }))
            {
                await daemon.Rebuild(projectionType, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
