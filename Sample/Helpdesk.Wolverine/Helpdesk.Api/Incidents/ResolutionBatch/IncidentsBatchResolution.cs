using System.Collections.Immutable;

namespace Helpdesk.Api.Incidents.ResolutionBatch;

public record IncidentsBatchResolution(
    Guid Id,
    ImmutableDictionary<Guid, ResolutionStatus> Incidents,
    ResolutionStatus Status,
    int Version = 0
)
{
    public static IncidentsBatchResolution Create(IncidentsBatchResolutionResolutionInitiated resolutionInitiated) =>
        new(
            resolutionInitiated.BatchId,
            resolutionInitiated.Incidents.ToImmutableDictionary(ks => ks, vs => ResolutionStatus.Pending),
            ResolutionStatus.Pending
        );

    public IncidentsBatchResolution Apply(IncidentResolutionRecorded resolved) =>
        this with { Incidents = Incidents.SetItem(resolved.IncidentId, ResolutionStatus.Resolved) };

    public IncidentsBatchResolution Apply(IncidentResolutionFailureRecorded resolutionFailed) =>
        this with { Incidents = Incidents.SetItem(resolutionFailed.IncidentId, ResolutionStatus.Failed) };

    public IncidentsBatchResolution Apply(IncidentsBatchResolutionResolutionCompleted resolutionCompleted) =>
        this with { Status = ResolutionStatus.Resolved };

    public IncidentsBatchResolution Apply(IncidentsBatchResolutionResolutionFailed resolutionFailed) =>
        this with { Status = ResolutionStatus.Failed };

    public IncidentsBatchResolution Apply(IncidentsBatchResolutionResolutionTimedOut resolutionTimedOut) =>
        this with { Status = ResolutionStatus.Failed };
}

public enum ResolutionStatus
{
    Pending,
    Resolved,
    Failed
}

public interface IncidentsBatchResolutionEvent;

public record IncidentsBatchResolutionResolutionInitiated(
    Guid BatchId,
    List<Guid> Incidents,
    Guid InitiatedBy,
    DateTimeOffset InitiatedAt
): IncidentsBatchResolutionEvent;

public record IncidentResolutionRecorded(
    Guid IncidentId,
    Guid BatchId,
    DateTimeOffset ResolvedAt
): IncidentsBatchResolutionEvent;

public record IncidentResolutionFailureRecorded(
    Guid IncidentId,
    Guid BatchId,
    DateTimeOffset FailedAt
): IncidentsBatchResolutionEvent;

public record IncidentsBatchResolutionResolutionCompleted(
    Guid BatchId,
    IReadOnlyList<Guid> Incidents,
    DateTimeOffset CompletedAt
): IncidentsBatchResolutionEvent;

public record IncidentsBatchResolutionResolutionFailed(
    Guid BatchId,
    ImmutableDictionary<Guid, ResolutionStatus> Incidents,
    DateTimeOffset FailedAt
): IncidentsBatchResolutionEvent;

public record IncidentsBatchResolutionResolutionTimedOut(
    Guid BatchId,
    ImmutableDictionary<Guid, ResolutionStatus> Incidents,
    DateTimeOffset TimedOutAt
): IncidentsBatchResolutionEvent;
