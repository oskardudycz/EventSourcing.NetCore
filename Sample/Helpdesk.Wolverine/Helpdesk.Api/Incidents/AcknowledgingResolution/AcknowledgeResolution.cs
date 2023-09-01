using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.AcknowledgingResolution;

public static class AcknowledgeResolutionEndpoint
{
    [WolverinePost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/acknowledge")]
    public static async Task<IResult> AcknowledgeResolution
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid customerId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new AcknowledgeResolution(incidentId, customerId, Now)), ct);

        return Ok();
    }

    public static ResolutionAcknowledgedByCustomer Handle(
        Incident current,
        AcknowledgeResolution command
    )
    {
        if (current.Status is not IncidentStatus.Resolved)
            throw new InvalidOperationException("Only resolved incident can be acknowledged");

        var (incidentId, acknowledgedBy, now) = command;

        return new ResolutionAcknowledgedByCustomer(incidentId, acknowledgedBy, now);
    }
}

public record AcknowledgeResolution(
    Guid IncidentId,
    Guid AcknowledgedBy,
    DateTimeOffset Now
);
