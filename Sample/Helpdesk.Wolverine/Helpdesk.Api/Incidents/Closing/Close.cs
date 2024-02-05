using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.Closing;

public static class CloseEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/close")]
    public static (IResult, Events) Close
    (
        CloseIncidentRequest request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
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

public record CloseIncidentRequest(
    Guid IncidentId
);
