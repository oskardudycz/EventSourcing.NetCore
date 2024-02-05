using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.AcknowledgingResolution;

public static class AcknowledgeResolutionEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/acknowledge")]
    public static (IResult, Events) AcknowledgeResolution
    (
        AcknowledgeResolutionRequest request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
        DateTimeOffset now
    )
    {
        if (incident.Status is not IncidentStatus.Resolved)
            throw new InvalidOperationException("Only resolved incident can be acknowledged");

        return (Ok(), [new ResolutionAcknowledgedByCustomer(incidentId, agentId, now)]);
    }
}

public record AcknowledgeResolutionRequest(
    Guid IncidentId // TODO: meh
);
