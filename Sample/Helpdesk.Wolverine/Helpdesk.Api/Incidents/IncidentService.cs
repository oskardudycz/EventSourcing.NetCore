namespace Helpdesk.Api.Incidents;

public record LogIncident(
    Guid IncidentId,
    Guid CustomerId,
    Contact Contact,
    string Description,
    Guid LoggedBy,
    DateTimeOffset Now
);

public record CategoriseIncident(
    Guid IncidentId,
    IncidentCategory Category,
    Guid CategorisedBy,
    DateTimeOffset Now
);

public record PrioritiseIncident(
    Guid IncidentId,
    IncidentPriority Priority,
    Guid PrioritisedBy,
    DateTimeOffset Now
);

public record AssignAgentToIncident(
    Guid IncidentId,
    Guid AgentId,
    DateTimeOffset Now
);

public record RecordAgentResponseToIncident(
    Guid IncidentId,
    IncidentResponse.FromAgent Response,
    DateTimeOffset Now
);

public record RecordCustomerResponseToIncident(
    Guid IncidentId,
    IncidentResponse.FromCustomer Response,
    DateTimeOffset Now
);

public record ResolveIncident(
    Guid IncidentId,
    ResolutionType Resolution,
    Guid ResolvedBy,
    DateTimeOffset Now
);

public record AcknowledgeResolution(
    Guid IncidentId,
    Guid AcknowledgedBy,
    DateTimeOffset Now
);

public record CloseIncident(
    Guid IncidentId,
    Guid ClosedBy,
    DateTimeOffset Now
);

internal static class IncidentService
{
    public static IncidentCategorised Handle(Incident current, CategoriseIncident command)
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, incidentCategory, categorisedBy, now) = command;

        return new IncidentCategorised(incidentId, incidentCategory, categorisedBy, now);
    }

    public static IncidentPrioritised Handle(Incident current, PrioritiseIncident command)
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, incidentPriority, prioritisedBy, now) = command;

        return new IncidentPrioritised(incidentId, incidentPriority, prioritisedBy, now);
    }

    public static AgentAssignedToIncident Handle(Incident current, AssignAgentToIncident command)
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, agentId, now) = command;

        return new AgentAssignedToIncident(incidentId, agentId, now);
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

    public static CustomerRespondedToIncident Handle(
        Incident current,
        RecordCustomerResponseToIncident command
    )
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, response, now) = command;

        return new CustomerRespondedToIncident(incidentId, response, now);
    }

    public static IncidentResolved Handle(
        Incident current,
        ResolveIncident command
    )
    {
        if (current.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
            throw new InvalidOperationException("Cannot resolve already resolved or closed incident");

        if (current.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot resolve incident that has outstanding responses to customer");

        var (incidentId, resolution, resolvedBy, now) = command;

        return new IncidentResolved(incidentId, resolution, resolvedBy, now);
    }

    public static ResolutionAcknowledgedByCustomer Handle(
        Incident current,
        AcknowledgeResolution command
    )
    {
        if (current.Status is not IncidentStatus.Resolved)
            throw new InvalidOperationException("Only resolved incident can be acknowledged");

        var (incidentId, acknowledgedBy, now) = command;

        return new ResolutionAcknowledgedByCustomer(incidentId, acknowledgedBy, now);
    }

    public static IncidentClosed Handle(
        Incident current,
        CloseIncident command
    )
    {
        if (current.Status is not IncidentStatus.ResolutionAcknowledgedByCustomer)
            throw new InvalidOperationException("Only incident with acknowledged resolution can be closed");

        if (current.HasOutstandingResponseToCustomer)
            throw new InvalidOperationException("Cannot close incident that has outstanding responses to customer");

        var (incidentId, acknowledgedBy, now) = command;

        return new IncidentClosed(incidentId, acknowledgedBy, now);
    }
}
