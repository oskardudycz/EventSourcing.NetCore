using System.Collections.Immutable;

namespace Helpdesk.Api.Incidents.ResolutionBatch;

public record IncidentsBatchResolution(
    Guid Id,
    ImmutableDictionary<Guid, ResolutionStatus> Incidents,
    ResolutionStatus Status
)
{
    public static IncidentsBatchResolution Create(IncidentsBatchResolutionInitiated initiated) =>
        new(
            initiated.BatchId,
            initiated.Incidents.ToImmutableDictionary(ks => ks, vs => ResolutionStatus.Pending),
            ResolutionStatus.Pending
        );

    public IncidentsBatchResolution Apply(IncidentResolutionRecorded resolved) =>
        this with { Incidents = Incidents.SetItem(resolved.IncidentId, ResolutionStatus.Resolved) };

    public IncidentsBatchResolution Apply(IncidentResolutionFailureRecorded resolutionFailed) =>
        this with { Incidents = Incidents.SetItem(resolutionFailed.IncidentId, ResolutionStatus.Failed) };

    public IncidentsBatchResolution Apply(IncidentsBatchResolutionCompleted completed) =>
        this with { Status = ResolutionStatus.Resolved };

    public IncidentsBatchResolution Apply(IncidentsBatchResolutionFailed failed) =>
        this with { Status = ResolutionStatus.Failed };

    public IncidentsBatchResolution Apply(IncidentsBatchResolutionTimedOut timedOut) =>
        this with { Status = ResolutionStatus.Failed };
}

public enum ResolutionStatus
{
    Pending,
    Resolved,
    Failed
}

public record IncidentsBatchResolutionInitiated(
    Guid BatchId,
    List<Guid> Incidents,
    Guid InitiatedBy,
    DateTimeOffset InitiatedAt
);

public record IncidentResolutionRecorded(
    Guid IncidentId,
    Guid BatchId,
    DateTimeOffset ResolvedAt
);

public record IncidentResolutionFailureRecorded(
    Guid IncidentId,
    Guid BatchId,
    DateTimeOffset FailedAt
);

public record IncidentsBatchResolutionCompleted(
    Guid BatchId,
    List<Guid> Incidents,
    DateTimeOffset CompletedAt
);

public record IncidentsBatchResolutionFailed(
    Guid BatchId,
    Dictionary<Guid, ResolutionStatus> Incidents,
    DateTimeOffset FailedAt
);

public record IncidentsBatchResolutionTimedOut(
    Guid BatchId,
    Dictionary<Guid, ResolutionStatus> Incidents,
    DateTimeOffset TimedOutAt
);
