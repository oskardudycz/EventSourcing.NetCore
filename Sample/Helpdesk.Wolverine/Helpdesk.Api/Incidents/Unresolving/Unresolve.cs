using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.Unresolving;

public static class UnResolveEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/unresolve")]
    public static (IResult, Events) Unresolve
    (
        UnesolveIncident command,
        Incident incident,
        DateTimeOffset now
    )
    {
        if (incident.Status is IncidentStatus.Closed)
            throw new InvalidOperationException("Cannot unresolve already closed incident");

        if (incident.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot resolve incident that has outstanding responses to customer");

        return (Ok(), [new IncidentUnresolved(incident.Id, command.Reason, command.AgentId, now)]);
    }
}

public record UnesolveIncident(
    Guid IncidentId,
    Guid AgentId,
    string Reason,
    int Version
);
