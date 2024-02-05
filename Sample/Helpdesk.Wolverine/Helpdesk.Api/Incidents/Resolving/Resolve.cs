using Microsoft.AspNetCore.Mvc;
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
        ResolveIncidentRequest request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
        DateTimeOffset now
    )
    {
        if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
            throw new InvalidOperationException("Cannot resolve already resolved or closed incident");

        if (incident.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot resolve incident that has outstanding responses to customer");

        return (Ok(), [new IncidentResolved(incidentId, request.Resolution, agentId, now)]);
    }
}

public record ResolveIncidentRequest(
    Guid IncidentId,
    ResolutionType Resolution
);
