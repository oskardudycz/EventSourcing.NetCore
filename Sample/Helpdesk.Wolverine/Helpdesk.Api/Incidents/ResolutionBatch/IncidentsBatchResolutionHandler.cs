using Helpdesk.Api.Incidents.Resolving;
using JasperFx.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api.Incidents.ResolutionBatch;

public static class IncidentsBatchResolutionHandler
{
    [WolverinePost("/api/agents/{agentId:guid}/incidents/resolve")]
    public static (CreationResponse, IStartStream, OutgoingMessages) InitiateResolutionBatch(
        InitiateIncidentsBatchResolution command,
        DateTimeOffset now
    )
    {
        var (incidents, agentId, resolutionType) = command;
        var batchId = CombGuidIdGeneration.NewGuid();

        var @event = new IncidentsBatchResolutionResolutionInitiated(batchId, incidents, agentId, now);

        var commands = new OutgoingMessages();
        commands.AddRange(incidents.Select(incidentId =>
            new ResolveIncidentFromBatch(incidentId, agentId, resolutionType, batchId))
        );

        return (
            new CreationResponse($"/api/incidents/resolution/{batchId}?version=1"),
            new StartStream<Incident>(batchId, @event),
            commands
        );
    }

    [AggregateHandler]
    public static Events Handle(IncidentResolved @event, IncidentsBatchResolution batch, DateTimeOffset now) =>
        @event.IncidentsBatchResolutionId != Guid.Empty
            ? Handle(batch, @event.IncidentId, ResolutionStatus.Resolved, now)
            : [];

    [AggregateHandler]
    public static Events Handle(IncidentResolutionFailed @event, IncidentsBatchResolution batch, DateTimeOffset now) =>
        @event.IncidentsBatchResolutionId != Guid.Empty
            ? Handle(batch, @event.IncidentId, ResolutionStatus.Failed, now)
            : [];

    private static Events Handle(
        IncidentsBatchResolution batch,
        Guid incidentId,
        ResolutionStatus status,
        DateTimeOffset now)
    {
        object recorded = status == ResolutionStatus.Resolved
            ? new IncidentResolutionRecorded(incidentId, batch.Id, now)
            : new IncidentResolutionFailureRecorded(incidentId, batch.Id, now);

        var incidents = batch.Incidents.SetItem(incidentId, status);

        var areAllFinished = incidents.Values.All(i => i != ResolutionStatus.Pending);

        if (!areAllFinished)
            return [recorded];

        object completed = incidents.Values.All(i => i == ResolutionStatus.Resolved)
            ? new IncidentsBatchResolutionResolutionCompleted(
                batch.Id,
                incidents.Keys.ToList(),
                now
            )
            : new IncidentsBatchResolutionResolutionFailed(batch.Id, batch.Incidents, now);

        return [recorded, completed];
    }
}

public record InitiateIncidentsBatchResolution(
    List<Guid> Incidents,
    Guid AgentId,
    ResolutionType ResolutionType
);
