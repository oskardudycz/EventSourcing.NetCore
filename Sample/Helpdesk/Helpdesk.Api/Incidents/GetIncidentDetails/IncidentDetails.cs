using Marten.Events.Aggregation;

namespace Helpdesk.Api.Incidents.GetIncidentDetails;

public record IncidentDetails(
    Guid Id,
    Guid CustomerId,
    IncidentStatus Status,
    IncidentNote[] Notes,
    IncidentCategory? Category = null,
    IncidentPriority? Priority = null,
    Guid? AgentId = null,
    int Version = 1
);

public record IncidentNote(
    IncidentNoteType Type,
    Guid From,
    string Content,
    bool VisibleToCustomer
);

public enum IncidentNoteType
{
    FromAgent,
    FromCustomer
}

public class IncidentDetailsProjection: SingleStreamProjection<IncidentDetails, Guid>
{
    public static IncidentDetails Create(IncidentLogged logged) =>
        new(logged.IncidentId, logged.CustomerId, IncidentStatus.Pending, []);

    public IncidentDetails Apply(IncidentCategorised categorised, IncidentDetails current) =>
        current with { Category = categorised.Category };

    public IncidentDetails Apply(IncidentPrioritised prioritised, IncidentDetails current) =>
        current with { Priority = prioritised.Priority };

    public IncidentDetails Apply(AgentAssignedToIncident prioritised, IncidentDetails current) =>
        current with { AgentId = prioritised.AgentId };

    public IncidentDetails Apply(AgentRespondedToIncident agentResponded, IncidentDetails current) =>
        current with
        {
            Notes = current.Notes.Union(
            [
                new IncidentNote(
                        IncidentNoteType.FromAgent,
                        agentResponded.Response.AgentId,
                        agentResponded.Response.Content,
                        agentResponded.Response.VisibleToCustomer
                    )
            ]).ToArray()
        };

    public IncidentDetails Apply(CustomerRespondedToIncident customerResponded, IncidentDetails current) =>
        current with
        {
            Notes = current.Notes.Union(
            [
                new IncidentNote(
                        IncidentNoteType.FromCustomer,
                        customerResponded.Response.CustomerId,
                        customerResponded.Response.Content,
                        true
                    )
            ]).ToArray()
        };

    public IncidentDetails Apply(IncidentResolved resolved, IncidentDetails current) =>
        current with { Status = IncidentStatus.Resolved };

    public IncidentDetails Apply(ResolutionAcknowledgedByCustomer acknowledged, IncidentDetails current) =>
        current with { Status = IncidentStatus.ResolutionAcknowledgedByCustomer };

    public IncidentDetails Apply(IncidentClosed closed, IncidentDetails current) =>
        current with { Status = IncidentStatus.Closed };
}
