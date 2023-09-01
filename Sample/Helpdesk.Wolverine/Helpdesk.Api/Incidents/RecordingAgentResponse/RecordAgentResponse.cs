using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.RecordingAgentResponse;

public static class RecordAgentResponseEndpoint
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/responses/")]
    public static async Task<IResult> RecordAgentResponseToIncident
    (
        IDocumentSession documentSession,
        [FromIfMatchHeader] string eTag,
        Guid incidentId,
        Guid agentId,
        RecordAgentResponseToIncidentRequest body,
        CancellationToken ct
    )
    {
        var (content, visibleToCustomer) = body;

        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state,
                new RecordAgentResponseToIncident(incidentId,
                    new IncidentResponse.FromAgent(agentId, content, visibleToCustomer), Now)), ct);

        return Ok();
    }

    public static AgentRespondedToIncident Handle(
        Incident current,
        RecordAgentResponseToIncident command
    )
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, response, now) = command;

        return new AgentRespondedToIncident(incidentId, response, now);
    }
}

public record RecordAgentResponseToIncident(
    Guid IncidentId,
    IncidentResponse.FromAgent Response,
    DateTimeOffset Now
);

public record RecordAgentResponseToIncidentRequest(
    string Content,
    bool VisibleToCustomer
);
