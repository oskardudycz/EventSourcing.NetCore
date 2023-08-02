using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Helpdesk.Api.Incidents.GetCustomerIncidentsSummary;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Helpdesk.Api.Incidents.GetIncidentHistory;
using Helpdesk.Api.Incidents.GetIncidentShortInfo;
using JasperFx.Core;
using Marten;
using Marten.AspNetCore;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Incidents.IncidentService;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents;

public static class IncidentsApi
{
    public static void MapIncidentsEndpoints(this WebApplication app)
    {
        app.MapPost("/api/customers/{customerId:guid}/incidents",
            async (
                IDocumentSession documentSession,
                Guid customerId,
                LogIncidentRequest body,
                CancellationToken ct) =>
            {
                var (contact, description) = body;
                var incidentId = CombGuidIdGeneration.NewGuid();

                await documentSession.Add<Incident>(incidentId,
                    Handle(new LogIncident(incidentId, customerId, contact, description, customerId, Now)), ct);

                return Created($"/api/incidents/{incidentId}", incidentId);
            }
        );

        app.MapPost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/category",
            (
                    IDocumentSession documentSession,
                    Guid incidentId,
                    Guid agentId,
                    [FromIfMatchHeader] string eTag,
                    CategoriseIncidentRequest body,
                    CancellationToken ct
                ) =>
                documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state, new CategoriseIncident(incidentId, body.Category, agentId, Now)), ct)
        );

        app.MapPost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/priority",
            (
                    IDocumentSession documentSession,
                    Guid incidentId,
                    Guid agentId,
                    [FromIfMatchHeader] string eTag,
                    PrioritiseIncidentRequest body,
                    CancellationToken ct
                ) =>
                documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state, new PrioritiseIncident(incidentId, body.Priority, agentId, Now)), ct)
        );

        app.MapPost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/assign",
            (
                    IDocumentSession documentSession,
                    Guid incidentId,
                    Guid agentId,
                    [FromIfMatchHeader] string eTag,
                    CancellationToken ct
                ) =>
                documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state, new AssignAgentToIncident(incidentId, agentId, Now)), ct)
        );

        app.MapPost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/responses/",
            (
                    IDocumentSession documentSession,
                    Guid incidentId,
                    Guid customerId,
                    [FromIfMatchHeader] string eTag,
                    RecordCustomerResponseToIncidentRequest body,
                    CancellationToken ct
                ) =>
                documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state,
                        new RecordCustomerResponseToIncident(incidentId,
                            new IncidentResponse.FromCustomer(customerId, body.Content), Now)), ct)
        );

        app.MapPost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/responses/",
            (
                IDocumentSession documentSession,
                [FromIfMatchHeader] string eTag,
                Guid incidentId,
                Guid agentId,
                RecordAgentResponseToIncidentRequest body,
                CancellationToken ct
            ) =>
            {
                var (content, visibleToCustomer) = body;

                return documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state,
                        new RecordAgentResponseToIncident(incidentId,
                            new IncidentResponse.FromAgent(agentId, content, visibleToCustomer), Now)), ct);
            }
        );

        app.MapPost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/resolve",
            (
                    IDocumentSession documentSession,
                    Guid incidentId,
                    Guid agentId,
                    [FromIfMatchHeader] string eTag,
                    ResolveIncidentRequest body,
                    CancellationToken ct
                ) =>
                documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state, new ResolveIncident(incidentId, body.Resolution, agentId, Now)), ct)
        );

        app.MapPost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/acknowledge",
            (
                    IDocumentSession documentSession,
                    Guid incidentId,
                    Guid customerId,
                    [FromIfMatchHeader] string eTag,
                    CancellationToken ct
                ) =>
                documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state, new AcknowledgeResolution(incidentId, customerId, Now)), ct)
        );

        app.MapPost("/api/agents/{agentId:guid}/incidents/{incidentId:guid}/close",
            async (
                IDocumentSession documentSession,
                Guid incidentId,
                Guid agentId,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
                    state => Handle(state, new CloseIncident(incidentId, agentId, Now)), ct);

                return Ok();
            }
        );

        app.MapGet("/api/customers/{customerId:guid}/incidents/",
            (IQuerySession querySession, Guid customerId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
                    CancellationToken ct) =>
                querySession.Query<IncidentShortInfo>().Where(i => i.CustomerId == customerId)
                    .ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct)
        );

        app.MapGet("/api/incidents/{incidentId:guid}",
            (HttpContext context, IQuerySession querySession, Guid incidentId) =>
                querySession.Json.WriteById<IncidentDetails>(incidentId, context)
        );

        app.MapGet("/api/incidents/{incidentId:guid}/history",
            (HttpContext context, IQuerySession querySession, Guid incidentId) =>
                querySession.Query<IncidentHistory>().Where(i => i.IncidentId == incidentId).WriteArray(context)
        );

        app.MapGet("/api/customers/{customerId:guid}/incidents/incidents-summary",
            (HttpContext context, IQuerySession querySession, Guid customerId) =>
                querySession.Json.WriteById<CustomerIncidentsSummary>(customerId, context)
        );
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
