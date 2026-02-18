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
        var batchId = Guid.CreateVersion7();

        var @event = new IncidentsBatchResolutionResolutionInitiated(batchId, incidents, agentId, now);

        var commands = new OutgoingMessages();
        commands.AddRange(incidents.Select(incidentId =>
            new ResolveIncidentFromBatch(incidentId, agentId, resolutionType, batchId))
        );
        commands.Add(new TimeoutIncidentsBatchResolution(batchId).DelayedFor(1.Minutes()));

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

    [AggregateHandler]
    public static Events Handle(TimeoutIncidentsBatchResolution command, IncidentsBatchResolution? batch,
        DateTimeOffset now) =>
        batch != null && batch.Status != ResolutionStatus.Pending
            ? [new IncidentsBatchResolutionResolutionTimedOut(command.IncidentsBatchResolutionId, batch.Incidents, now)]
            : [];

    private static Events Handle(
        IncidentsBatchResolution batch,
        Guid incidentId,
        ResolutionStatus status,
        DateTimeOffset now)
    {
        if (batch.Status != ResolutionStatus.Pending || batch.Incidents[incidentId] != ResolutionStatus.Pending)
            return [];

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

public record TimeoutIncidentsBatchResolution(
    Guid IncidentsBatchResolutionId
);
