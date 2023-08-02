using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using JasperFx.Core;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Incidents.IncidentService;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents;

public static class IncidentsEndpoints
{
    [WolverinePost("/api/customers/{customerId:guid}/incidents")]
    public static async Task<IResult> LogIncident
    (
        Guid customerId,
        LogIncidentRequest body,
        IDocumentSession documentSession,
        CancellationToken ct)
    {
        var (contact, description) = body;
        var incidentId = CombGuidIdGeneration.NewGuid();

        await documentSession.Add<Incident>(incidentId,
            Handle(new LogIncident(incidentId, customerId, contact, description, customerId, Now)), ct);

        return Created($"/api/incidents/{incidentId}", incidentId);
    }

    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/category")]
    public static async Task<IResult> CategoriseIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        CategoriseIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new CategoriseIncident(incidentId, body.Category, agentId, Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/priority")]
    public static async Task<IResult> PrioritiseIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        PrioritiseIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new PrioritiseIncident(incidentId, body.Priority, agentId, Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/assign")]
    public static async Task<IResult> AssignAgentToIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new AssignAgentToIncident(incidentId, agentId, Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/responses/")]
    public static async Task<IResult> RecordCustomerResponseToIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid customerId,
        [FromIfMatchHeader] string eTag,
        RecordCustomerResponseToIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state,
                new RecordCustomerResponseToIncident(incidentId,
                    new IncidentResponse.FromCustomer(customerId, body.Content), Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/responses/")]
    public static async Task<IResult> RecordAgentResponseToIncident
    (
        IDocumentSession documentSession,
        [FromIfMatchHeader] string eTag,
        Guid incidentId,
        Guid agentId,
        RecordAgentResponseToIncidentRequest body,
        CancellationToken ct
    )
    {
        var (content, visibleToCustomer) = body;

        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state,
                new RecordAgentResponseToIncident(incidentId,
                    new IncidentResponse.FromAgent(agentId, content, visibleToCustomer), Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/resolve")]
    public static async Task<IResult> ResolveIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        ResolveIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new ResolveIncident(incidentId, body.Resolution, agentId, Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/acknowledge")]
    public static async Task<IResult> AcknowledgeResolution
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid customerId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new AcknowledgeResolution(incidentId, customerId, Now)), ct);

        return Ok();
    }

    [WolverinePost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/close")]
    public static async Task<IResult> CloseIncident
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new CloseIncident(incidentId, agentId, Now)), ct);

        return Ok();
    }
}

public record LogIncidentRequest(
    Contact Contact,
    string Description
);

public record CategoriseIncidentRequest(
    IncidentCategory Category
);

public record PrioritiseIncidentRequest(
    IncidentPriority Priority
);

public record RecordCustomerResponseToIncidentRequest(
    string Content
);

public record RecordAgentResponseToIncidentRequest(
    string Content,
    bool VisibleToCustomer
);

public record ResolveIncidentRequest(
    ResolutionType Resolution
);
