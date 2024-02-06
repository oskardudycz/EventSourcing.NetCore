using Marten.Events.Aggregation;

namespace Helpdesk.Api.Incidents.GettingCustomerIncidents;

public record IncidentShortInfo(
    Guid Id,
    Guid CustomerId,
    IncidentStatus Status,
    int NotesCount,
    IncidentCategory? Category = null,
    IncidentPriority? Priority = null
);

public class IncidentShortInfoProjection: SingleStreamProjection<IncidentShortInfo>
{
    public static IncidentShortInfo Create(IncidentLogged logged) =>
        new(logged.IncidentId, logged.CustomerId, IncidentStatus.Pending, 0);

    public IncidentShortInfo Apply(IncidentCategorised categorised, IncidentShortInfo current) =>
        current with { Category = categorised.Category };

    public IncidentShortInfo Apply(IncidentPrioritised prioritised, IncidentShortInfo current) =>
        current with { Priority = prioritised.Priority };

    public IncidentShortInfo Apply(AgentRespondedToIncident agentResponded, IncidentShortInfo current) =>
        current with { NotesCount = current.NotesCount + 1 };

    public IncidentShortInfo Apply(CustomerRespondedToIncident customerResponded, IncidentShortInfo current) =>
        current with { NotesCount = current.NotesCount + 1 };

    public IncidentShortInfo Apply(IncidentResolved resolved, IncidentShortInfo current) =>
        current with { Status = IncidentStatus.Resolved };

    public IncidentShortInfo Apply(IncidentUnresolved unresolved, IncidentShortInfo current) =>
        current with { Status = IncidentStatus.Pending, NotesCount = current.NotesCount + 1 };

    public IncidentShortInfo Apply(ResolutionAcknowledgedByCustomer acknowledged, IncidentShortInfo current) =>
        current with { Status = IncidentStatus.ResolutionAcknowledgedByCustomer };

    public IncidentShortInfo Apply(IncidentClosed closed, IncidentShortInfo current) =>
        current with { Status = IncidentStatus.Closed };
}
