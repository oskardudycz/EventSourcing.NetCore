using System.Text.Json.Serialization;
using Helpdesk;
using Helpdesk.Api;
using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Helpdesk.Incidents;
using Helpdesk.Incidents.GetCustomerIncidentsSummary;
using Helpdesk.Incidents.GetIncidentDetails;
using Helpdesk.Incidents.GetIncidentHistory;
using Helpdesk.Incidents.GetIncidentShortInfo;
using Marten;
using Marten.AspNetCore;
using Marten.Pagination;
using Marten.Schema.Identity;
using Microsoft.AspNetCore.Mvc;
using Oakton;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMartenForHelpdeskInlineOnly(builder.Configuration);

builder.Services
    .Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

var customersIncidents = app.MapGroup("api/customers/{customerId:guid}/incidents/").WithTags("Customer", "Incident");
var agentIncidents = app.MapGroup("api/agents/{agentId:guid}/incidents/").WithTags("Agent", "Incident");
var incidents = app.MapGroup("api/incidents").WithTags("Incident");

customersIncidents.MapPost("",
    async (
        IDocumentSession documentSession,
        Guid customerId,
        LogIncidentRequest body,
        CancellationToken ct) =>
    {
        var (contact, description) = body;
        var incidentId = CombGuidIdGeneration.NewGuid();

        await documentSession.Add<Incident>(incidentId,
            IncidentService.Handle(new LogIncident(incidentId, customerId, contact, description, customerId, DateTimeOffset.Now)), ct);

        return TypedResults.Created($"/api/incidents/{incidentId}", incidentId);
    }
);

agentIncidents.MapPost("{incidentId:guid}/category",
    (
            IDocumentSession documentSession,
            Guid incidentId,
            Guid agentId,
            [FromIfMatchHeader] string eTag,
            CategoriseIncidentRequest body,
            CancellationToken ct
        ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state, new CategoriseIncident(incidentId, body.Category, agentId, DateTimeOffset.Now)), ct)
);

agentIncidents.MapPost("{incidentId:guid}/priority",
    (
            IDocumentSession documentSession,
            Guid incidentId,
            Guid agentId,
            [FromIfMatchHeader] string eTag,
            PrioritiseIncidentRequest body,
            CancellationToken ct
        ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state, new PrioritiseIncident(incidentId, body.Priority, agentId, DateTimeOffset.Now)), ct)
);

agentIncidents.MapPost("{incidentId:guid}/assign",
    (
            IDocumentSession documentSession,
            Guid incidentId,
            Guid agentId,
            [FromIfMatchHeader] string eTag,
            CancellationToken ct
        ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state, new AssignAgentToIncident(incidentId, agentId, DateTimeOffset.Now)), ct)
);

customersIncidents.MapPost("{incidentId:guid}/responses/",
    (
            IDocumentSession documentSession,
            Guid incidentId,
            Guid customerId,
            [FromIfMatchHeader] string eTag,
            RecordCustomerResponseToIncidentRequest body,
            CancellationToken ct
        ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state,
                new RecordCustomerResponseToIncident(incidentId,
                    new IncidentResponse.FromCustomer(customerId, body.Content), DateTimeOffset.Now)), ct)
);

agentIncidents.MapPost("{incidentId:guid}/responses/",
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

        return documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state,
                new RecordAgentResponseToIncident(incidentId,
                    new IncidentResponse.FromAgent(agentId, content, visibleToCustomer), DateTimeOffset.Now)), ct);
    }
);

agentIncidents.MapPost("{incidentId:guid}/resolve",
    (
            IDocumentSession documentSession,
            Guid incidentId,
            Guid agentId,
            [FromIfMatchHeader] string eTag,
            ResolveIncidentRequest body,
            CancellationToken ct
        ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state, new ResolveIncident(incidentId, body.Resolution, agentId, DateTimeOffset.Now)), ct)
);

customersIncidents.MapPost("{incidentId:guid}/acknowledge",
    (
            IDocumentSession documentSession,
            Guid incidentId,
            Guid customerId,
            [FromIfMatchHeader] string eTag,
            CancellationToken ct
        ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state, new AcknowledgeResolution(incidentId, customerId, DateTimeOffset.Now)), ct)
);

agentIncidents.MapPost("{incidentId:guid}/close",
    async (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct) =>
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ETagExtensions.ToExpectedVersion(eTag),
            state => IncidentService.Handle(state, new CloseIncident(incidentId, agentId, DateTimeOffset.Now)), ct);

        return TypedResults.Ok();
    }
);

customersIncidents.MapGet("",
    (IQuerySession querySession, Guid customerId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
            CancellationToken ct) =>
        querySession.Query<IncidentShortInfo>().Where(i => i.CustomerId == customerId)
            .ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct)
);

incidents.MapGet("{incidentId:guid}",
    (HttpContext context, IQuerySession querySession, Guid incidentId) =>
        querySession.Json.WriteById<IncidentDetails>(incidentId, context)
);

incidents.MapGet("{incidentId:guid}/history",
    (HttpContext context, IQuerySession querySession, Guid incidentId) =>
        querySession.Query<IncidentHistory>().Where(i => i.IncidentId == incidentId).WriteArray(context)
);

customersIncidents.MapGet("incidents-summary",
    (HttpContext context, IQuerySession querySession, Guid customerId) =>
        querySession.Json.WriteById<CustomerIncidentsSummary>(customerId, context)
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

return await app.RunOaktonCommands(args);

namespace Helpdesk.Api
{
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

    public partial class Program
    {
    }
}
