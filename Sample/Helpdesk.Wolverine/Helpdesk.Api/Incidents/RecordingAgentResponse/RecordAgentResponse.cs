using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.RecordingAgentResponse;

public static class RecordAgentResponseEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/responses/")]
    public static (IResult, Events) RecordAgentResponse
    (
        RecordAgentResponseToIncident command,
        Incident incident,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var response = new IncidentResponse.FromAgent(
            command.AgentId, command.Content, command.VisibleToCustomer
        );

        return (Ok(), [new AgentRespondedToIncident(incident.Id, response, now)]);
    }
}

public record RecordAgentResponseToIncident(
    Guid IncidentId,
    Guid AgentId,
    string Content,
    bool VisibleToCustomer,
    int Version
);
