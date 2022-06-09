using System.Text.Json.Serialization;
using Helpdesk.Api.Core.Kafka;
using Helpdesk.Api.Core.Marten;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetCustomerIncidentsSummary;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Helpdesk.Api.Incidents.GetIncidentHistory;
using Helpdesk.Api.Incidents.GetIncidentShortInfo;
using Marten;
using Marten.AspNetCore;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Pagination;
using Marten.Schema.Identity;
using Microsoft.AspNetCore.Mvc;
using Weasel.Core;
using static Microsoft.AspNetCore.Http.Results;
using static Helpdesk.Api.Incidents.IncidentService;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMarten(options =>
    {
        options.Connection(builder.Configuration.GetConnectionString("Incidents"));
        options.UseDefaultSerialization(EnumStorage.AsString, nonPublicMembersStorage: NonPublicMembersStorage.All);

        options.Projections.Add<IncidentHistoryTransformation>();
        options.Projections.Add<IncidentDetailsProjection>();
        options.Projections.Add<IncidentShortInfoProjection>();
        options.Projections.Add<CustomerIncidentsSummaryProjection>(ProjectionLifecycle.Async);
        options.Projections.Add(new KafkaProducer(builder.Configuration), ProjectionLifecycle.Async);
    }).AddAsyncDaemon(DaemonMode.Solo);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapPost("api/customers/{customerId:guid}/incidents/",
    async (
        IDocumentSession documentSession,
        Guid customerId,
        LogIncidentRequest body,
        CancellationToken ct) =>
    {
        var (contact, description) = body;
        var incidentId = CombGuidIdGeneration.NewGuid();

        await documentSession.Add<Incident>(incidentId,
            Handle(new LogIncident(incidentId, customerId, contact, description, customerId)), ct);

        return Created($"/api/incidents/{incidentId}", incidentId);
    }
).WithTags("Customer");

app.MapPost("api/agents/{agentId:guid}/incidents/{incidentId:guid}/category",
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromHeader(Name = "If-Match")] string eTag,
        CategoriseIncidentRequest body,
        CancellationToken ct
    ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current, new CategoriseIncident(incidentId, body.Category, agentId)), ct)
).WithTags("Agent");;

app.MapPost("api/agents/{agentId:guid}/incidents/{incidentId:guid}/priority",
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromHeader(Name = "If-Match")] string eTag,
        PrioritiseIncidentRequest body,
        CancellationToken ct
    ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current, new PrioritiseIncident(incidentId, body.Priority, agentId)), ct)
).WithTags("Agent");

app.MapPut("api/agents/{agentId:guid}/incidents/{incidentId:guid}/assign",
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromHeader(Name = "If-Match")] string eTag,
        CancellationToken ct
    ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current, new AssignAgent(incidentId, agentId)), ct)
).WithTags("Agent");

app.MapPost("api/customers/{customerId:guid}/incidents/{incidentId:guid}/responses/",
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid customerId,
        [FromHeader(Name = "If-Match")] string eTag,
        RecordCustomerResponseToIncidentRequest body,
        CancellationToken ct
    ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current,
                new RecordCustomerResponseToIncident(incidentId,
                    new IncidentResponse.FromCustomer(customerId, body.Content))), ct)
).WithTags("Customer");

app.MapPost("api/agents/{agentId:guid}/incidents/{incidentId:guid}/responses/",
    (
        IDocumentSession documentSession,
        [FromHeader(Name = "If-Match")] string eTag,
        Guid incidentId,
        Guid agentId,
        RecordAgentResponseToIncidentRequest body,
        CancellationToken ct
    ) =>
    {
        var (content, visibleToCustomer) = body;

        return documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current,
                new RecordAgentResponseToIncident(incidentId,
                    new IncidentResponse.FromAgent(agentId, content, visibleToCustomer))), ct);
    }
).WithTags("Agent");

app.MapPost("api/agents/{agentId:guid}/incidents/{incidentId:guid}/resolve",
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromHeader(Name = "If-Match")] string eTag,
        ResolveIncidentRequest body,
        CancellationToken ct
    ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current, new ResolveIncident(incidentId, body.Resolution, agentId)), ct)
).WithTags("Agent");

app.MapPost("api/customers/{customerId:guid}/incidents/{incidentId:guid}/acknowledge",
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid customerId,
        [FromHeader(Name = "If-Match")] string eTag,
        CancellationToken ct
    ) =>
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current, new AcknowledgeResolution(incidentId, customerId)), ct)
).WithTags("Customer");

app.MapPost("api/agents/{agentId:guid}/incidents/{incidentId:guid}/close",
    async (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid agentId,
        [FromHeader(Name = "If-Match")] string eTag,
        CancellationToken ct) =>
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            current => Handle(current, new CloseIncident(incidentId, agentId)), ct);

        return Ok();
    }
).WithTags("Agent");

app.MapGet("api/customers/{customerId:guid}/incidents/",
    (IQuerySession querySession, Guid customerId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
            CancellationToken ct) =>
        querySession.Query<IncidentShortInfo>().Where(i => i.CustomerId == customerId)
            .ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct)
).WithTags("Customer");

app.MapGet("api/incidents/{incidentId:guid}",
    (HttpContext context, IQuerySession querySession, Guid incidentId) =>
        querySession.Json.WriteById<IncidentDetails>(incidentId, context)
).WithTags("Incident");

app.MapGet("api/incidents/{incidentId:guid}/history",
    (HttpContext context, IQuerySession querySession, Guid incidentId) =>
        querySession.Query<IncidentHistory>().Where(i => i.IncidentId == incidentId).WriteArray(context)
).WithTags("Incident");

app.MapGet("api/customers/{customerId:guid}/incidents-summary",
    (HttpContext context, IQuerySession querySession, Guid customerId) =>
        querySession.Json.WriteById<CustomerIncidentsSummary>(customerId, context)
).WithTags("Customer");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

app.Run();

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
