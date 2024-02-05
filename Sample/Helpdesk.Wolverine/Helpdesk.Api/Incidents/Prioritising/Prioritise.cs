using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.Prioritising;

public static class PrioritiseEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/priority")]
    public static (IResult, Events) Prioritise(
        PrioritiseIncident command,
        Incident incident,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        return (Ok(), [new IncidentPrioritised(command.IncidentId, command.Priority, command.AgentId, now)]);
    }
}

public record PrioritiseIncident(
    Guid IncidentId,
    Guid AgentId,
    IncidentPriority Priority,
    int Version
);
