using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.Closing;

public static class CloseEndpoint
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/close")]
    public static async Task<IResult> CloseIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new CloseIncident(incidentId, agentId, Now)), ct);

        return Ok();
    }

    public static IncidentClosed Handle(
        Incident current,
        CloseIncident command
    )
    {
        if (current.Status is not IncidentStatus.ResolutionAcknowledgedByCustomer)
            throw new InvalidOperationException("Only incident with acknowledged resolution can be closed");

        if (current.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot close incident that has outstanding responses to customer");

        var (incidentId, acknowledgedBy, now) = command;

        return new IncidentClosed(incidentId, acknowledgedBy, now);
    }
}

public record CloseIncident(
    Guid IncidentId,
    Guid ClosedBy,
    DateTimeOffset Now
);
