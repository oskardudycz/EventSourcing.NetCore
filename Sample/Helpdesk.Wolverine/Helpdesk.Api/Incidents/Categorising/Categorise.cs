using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.Categorising;

public static class CategoriseEndpoint
{
    [AggregateHandler]
    [WolverinePost("api/agents/{agentId:guid}/incidents/{incidentId:guid}/category")]
    public static (IResult, Events) Categorise
    (
        CategoriseIncidentRequest request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        return (Ok(), [new IncidentCategorised(incidentId, request.Category, agentId, now)]);
    }
}

public record CategoriseIncidentRequest(
    Guid IncidentId, // TODO: meh
    IncidentCategory Category
);
