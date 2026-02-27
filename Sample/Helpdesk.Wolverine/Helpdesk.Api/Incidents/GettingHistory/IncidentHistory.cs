using JasperFx.Events;
using Marten.Events.Projections;

namespace Helpdesk.Api.Incidents.GettingHistory;

public record IncidentHistory(
    Guid Id,
    Guid IncidentId,
    string Description
);

public class IncidentHistoryTransformation: EventProjection
{
    public IncidentHistory Transform(IEvent<IncidentLogged> input)
    {
        var (incidentId, customerId, contact, description, loggedBy, loggedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"['{loggedAt}'] Logged Incident with id: '{incidentId}' for customer '{customerId}' and description `{description}' through {contact} by '{loggedBy}'"
        );
    }

    public IncidentHistory Transform(IEvent<IncidentCategorised> input)
    {
        var (incidentId, category, categorisedBy, categorisedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{categorisedAt}] Categorised Incident with id: '{incidentId}' as {category} by {categorisedBy}"
        );
    }

    public IncidentHistory Transform(IEvent<IncidentPrioritised> input)
    {
        var (incidentId, priority, prioritisedBy, prioritisedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{prioritisedAt}] Prioritised Incident with id: '{incidentId}' as '{priority}' by {prioritisedBy}"
        );
    }

    public IncidentHistory Transform(IEvent<AgentAssignedToIncident> input)
    {
        var (incidentId, agentId, assignedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{assignedAt}] Assigned agent `{agentId} to incident with id: '{incidentId}'"
        );
    }

    public IncidentHistory Transform(IEvent<CustomerRespondedToIncident> input)
    {
        var (incidentId, response, respondedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{respondedAt}] Agent '{response.CustomerId}' responded with response '{response.Content}' to Incident with id: '{incidentId}'"
        );
    }

    public IncidentHistory Transform(IEvent<AgentRespondedToIncident> input)
    {
        var (incidentId, response, respondedAt) = input.Data;

        var responseVisibility = response.VisibleToCustomer ? "public" : "private";

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{respondedAt}] Agent '{response.AgentId}' responded with {responseVisibility} response '{response.Content}' to Incident with id: '{incidentId}'"
        );
    }

    public IncidentHistory Transform(IEvent<IncidentResolved> input)
    {
        var (incidentId, resolution, resolvedBy, resolvedAt, _) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{resolvedAt}] Resolved Incident with id: '{incidentId}' with resolution `{resolution} by '{resolvedBy}'"
        );
    }

    public IncidentHistory Transform(IEvent<IncidentUnresolved> input)
    {
        var (incidentId, reason, unresolvedBy, resolvedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{resolvedAt}] Unresolved Incident with id: '{incidentId}' with reason `{reason} by '{unresolvedBy}'"
        );
    }

    public IncidentHistory Transform(IEvent<ResolutionAcknowledgedByCustomer> input)
    {
        var (incidentId, acknowledgedBy, acknowledgedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{acknowledgedAt}] Customer '{acknowledgedBy}' acknowledged resolution of Incident with id: '{incidentId}'"
        );
    }

    public IncidentHistory Transform(IEvent<IncidentClosed> input)
    {
        var (incidentId, closedBy, closedAt) = input.Data;

        return new IncidentHistory(
            Guid.CreateVersion7(),
            incidentId,
            $"[{closedAt}] Agent '{closedBy}' closed Incident with id: '{incidentId}'"
        );
    }
}
