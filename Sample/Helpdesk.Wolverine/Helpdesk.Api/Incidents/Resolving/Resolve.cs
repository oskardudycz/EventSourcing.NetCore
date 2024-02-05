using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.Resolving;

public static class ResolveEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/resolve")]
    public static (IResult, Events) Resolve
    (
        ResolveIncident command,
        Incident incident,
        DateTimeOffset now
    )
    {
        if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
            throw new InvalidOperationException("Cannot resolve already resolved or closed incident");

        if (incident.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot resolve incident that has outstanding responses to customer");

        return (Ok(), [new IncidentResolved(incident.Id, command.Resolution, command.AgentId, now)]);
    }
}

public record ResolveIncident(
    Guid IncidentId,
    Guid AgentId,
    ResolutionType Resolution,
    int Version
);
