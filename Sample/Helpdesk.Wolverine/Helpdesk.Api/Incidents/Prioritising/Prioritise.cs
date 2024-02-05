using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.Prioritising;

public static class PrioritiseEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/priority")]
    public static (IResult, Events) Prioritise(
        PrioritiseIncidentRequest request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        return (Ok(), [new IncidentPrioritised(incidentId, request.Priority, agentId, now)]);
    }
}

public record PrioritiseIncidentRequest(
    Guid IncidentId, // TODO: meh
    IncidentPriority Priority
);
