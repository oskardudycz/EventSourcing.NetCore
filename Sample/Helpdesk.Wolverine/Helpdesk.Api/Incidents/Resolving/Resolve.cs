using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.Resolving;

public static class ResolveEndpoint
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/resolve")]
    public static async Task<IResult> ResolveIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        ResolveIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new ResolveIncident(incidentId, body.Resolution, agentId, Now)), ct);

        return Ok();
    }

    public static IncidentResolved Handle(
        Incident current,
        ResolveIncident command
    )
    {
        if (current.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
            throw new InvalidOperationException("Cannot resolve already resolved or closed incident");

        if (current.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot resolve incident that has outstanding responses to customer");

        var (incidentId, resolution, resolvedBy, now) = command;

        return new IncidentResolved(incidentId, resolution, resolvedBy, now);
    }
}

public record ResolveIncidentRequest(
    ResolutionType Resolution
);

public record ResolveIncident(
    Guid IncidentId,
    ResolutionType Resolution,
    Guid ResolvedBy,
    DateTimeOffset Now
);
