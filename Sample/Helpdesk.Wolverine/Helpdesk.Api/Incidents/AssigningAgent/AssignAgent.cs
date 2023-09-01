using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.AssigningAgent;

public static class AssignAgentEndpoint
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/assign")]
    public static async Task<IResult> AssignAgent
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new AssignAgentToIncident(incidentId, agentId, Now)), ct);

        return Ok();
    }

    public static AgentAssignedToIncident Handle(Incident current, AssignAgentToIncident command)
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, agentId, now) = command;

        return new AgentAssignedToIncident(incidentId, agentId, now);
    }
}

public record AssignAgentToIncident(
    Guid IncidentId,
    Guid AgentId,
    DateTimeOffset Now
);
