using JasperFx.Core;
using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api.Incidents.ResolutionBatch;

public static class IncidentsBatchResolutionSaga
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/resolve")]
    public static (CreationResponse, IStartStream) InitiateResolutionBatch(
        InitiateIncidentsBatchResolution command,
        DateTimeOffset now
    )
    {
        var (incidents, agentId) = command;
        var batchId = CombGuidIdGeneration.NewGuid();

        var @event = new IncidentsBatchResolutionInitiated(batchId, incidents, agentId, now);

        return (
            new CreationResponse($"/api/incidents/resolution/{batchId}"),
            new StartStream<Incident>(batchId, @event)
        );
    }
}

public record InitiateIncidentsBatchResolution(
    List<Guid> Incidents,
    Guid AgentId
);


