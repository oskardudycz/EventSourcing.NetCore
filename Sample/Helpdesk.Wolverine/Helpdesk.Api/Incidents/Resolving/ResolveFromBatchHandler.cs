using Wolverine;
using Wolverine.Marten;

namespace Helpdesk.Api.Incidents.Resolving;

public static class ResolveFromBatchHandler
{
    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle
    (
        ResolveIncidentFromBatch command,
        Incident incident,
        DateTimeOffset now
    )
    {
        if (incident.Status is IncidentStatus.Resolved
            or IncidentStatus.ResolutionAcknowledgedByCustomer
            or IncidentStatus.Closed
           )
            return ([],
            [
                new IncidentResolutionFailed(incident.Id, command.BatchId,
                    IncidentResolutionFailed.Reason.AlreadyResolved)
            ]);

        if (incident.HasOutstandingResponseToCustomer)
            return ([],
            [
                new IncidentResolutionFailed(incident.Id, command.BatchId,
                    IncidentResolutionFailed.Reason.HasOutstandingResponseToCustomer)
            ]);

        return ([new IncidentResolved(incident.Id, command.Resolution, command.AgentId, now, command.BatchId)], []);
    }
}

public record ResolveIncidentFromBatch(
    Guid IncidentId,
    Guid AgentId,
    ResolutionType Resolution,
    Guid BatchId
);

public record IncidentResolutionFailed(
    Guid IncidentId,
    Guid IncidentsBatchResolutionId,
    IncidentResolutionFailed.Reason FailureReason
)
{
    public enum Reason
    {
        AlreadyResolved,
        HasOutstandingResponseToCustomer
    }
}
