using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.Closing;

public static class CloseEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/close")]
    public static (IResult, Events) Close
    (
        CloseIncident request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,

        DateTimeOffset now
    )
    {
        if (incident.Status is not IncidentStatus.ResolutionAcknowledgedByCustomer)
            throw new InvalidOperationException("Only incident with acknowledged resolution can be closed");

        if (incident.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot close incident that has outstanding responses to customer");

        return (Ok(), [new IncidentClosed(incidentId, agentId, now)]);
    }
}

public record CloseIncident(
    Guid IncidentId
);
