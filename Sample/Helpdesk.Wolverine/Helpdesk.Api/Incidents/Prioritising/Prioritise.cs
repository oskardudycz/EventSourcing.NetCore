using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.Prioritising;

public static class PrioritiseEndpoint
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/priority")]
    public static async Task<IResult> Prioritise
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        PrioritiseIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new PrioritiseIncident(incidentId, body.Priority, agentId, Now)), ct);

        return Ok();
    }

    public static IncidentPrioritised Handle(Incident current, PrioritiseIncident command)
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, incidentPriority, prioritisedBy, now) = command;

        return new IncidentPrioritised(incidentId, incidentPriority, prioritisedBy, now);
    }
}

public record PrioritiseIncidentRequest(
    IncidentPriority Priority
);


public record PrioritiseIncident(
    Guid IncidentId,
    IncidentPriority Priority,
    Guid PrioritisedBy,
    DateTimeOffset Now
);
