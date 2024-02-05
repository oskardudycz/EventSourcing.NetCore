using Microsoft.AspNetCore.Mvc;
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
        RecordAgentResponseToIncidentRequest request,
        Incident incident,
        [FromRoute] Guid agentId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var response = new IncidentResponse.FromAgent(
            agentId, request.Content, request.VisibleToCustomer
        );

        return (Ok(), [new AgentRespondedToIncident(incidentId, response, now)]);
    }
}

public record RecordAgentResponseToIncidentRequest(
    Guid IncidentId,
    string Content,
    bool VisibleToCustomer
);
