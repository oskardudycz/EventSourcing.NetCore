using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.AssigningAgent;

public static class AssignAgentEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/assign")]
    public static (IResult, Events) AssignAgent(
        AssignAgentToIncident toIncident,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,

        DateTimeOffset now)
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        return (Ok(), [new AgentAssignedToIncident(incidentId, agentId, now)]);
    }
}

public record AssignAgentToIncident(
    Guid IncidentId,
    int Version
);
