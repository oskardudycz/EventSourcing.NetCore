using System.Text.Json.Serialization;
using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Kafka;
using Helpdesk.Api.Core.Marten;
using Helpdesk.Api.Core.SignalR;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetCustomerIncidentsSummary;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Helpdesk.Api.Incidents.GetIncidentHistory;
using Helpdesk.Api.Incidents.GetIncidentShortInfo;
using JasperFx.CodeGeneration;
using Marten;
using Marten.AspNetCore;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Pagination;
using Marten.Schema.Identity;
using Marten.Storage;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Oakton;
using Weasel.Core;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Incidents.IncidentService;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5248");
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMarten(sp =>
    {
        var options = new StoreOptions();

        var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "Helpdesk";
        options.Events.DatabaseSchemaName = schemaName;
        options.DatabaseSchemaName = schemaName;
        options.Connection(builder.Configuration.GetConnectionString("Incidents") ??
                           throw new InvalidOperationException());

        options.UseSystemTextJsonForSerialization(EnumStorage.AsString);

        options.Events.TenancyStyle = TenancyStyle.Conjoined;
        options.Policies.AllDocumentsAreMultiTenanted();

        options.Events.StreamIdentity = StreamIdentity.AsString;
        options.Events.MetadataConfig.HeadersEnabled = true;
        options.Events.MetadataConfig.CausationIdEnabled = true;
        options.Events.MetadataConfig.CorrelationIdEnabled = true;


        options.Projections.Errors.SkipApplyErrors = false;
        options.Projections.Errors.SkipSerializationErrors = false;
        options.Projections.Errors.SkipUnknownEvents = false;

        // options.Projections.LiveStreamAggregation<Incident>();
        // options.Projections.Add<IncidentHistoryTransformation>(ProjectionLifecycle.Inline);
        // options.Projections.Add<IncidentDetailsProjection>(ProjectionLifecycle.Inline);
        // options.Projections.Add<IncidentShortInfoProjection>(ProjectionLifecycle.Inline);
        // options.Projections.Add<CustomerIncidentsSummaryProjection>(ProjectionLifecycle.Async);

        options.ApplicationAssembly = typeof(CustomerIncidentsSummaryProjection).Assembly;

        return options;
    })
    .AddSubscriptionWithServices<KafkaProducer>(ServiceLifetime.Singleton)
    .AddSubscriptionWithServices<SignalRProducer<IncidentsHub>>(ServiceLifetime.Singleton)
    .OptimizeArtifactWorkflow(TypeLoadMode.Static)
    .ApplyAllDatabaseChangesOnStartup()
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.HotCold);


// Header forwarding to enable Swagger in Nginx
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services
    .AddCors(options =>
        options.AddPolicy("ClientPermission", policy =>
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
        )
    )
    .Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddSignalR();

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
            Handle(new LogIncident(incidentId, customerId, contact, description, customerId, Now)), ct);

        return Created($"/api/incidents/{incidentId}", incidentId);
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
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new CategoriseIncident(incidentId, body.Category, agentId, Now)), ct)
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
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new PrioritiseIncident(incidentId, body.Priority, agentId, Now)), ct)
);

agentIncidents.MapPost("{incidentId:guid}/assign",
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

customersIncidents.MapPost("{incidentId:guid}/responses/",
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

        return documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state,
                new RecordAgentResponseToIncident(incidentId,
                    new IncidentResponse.FromAgent(agentId, content, visibleToCustomer), Now)), ct);
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
        documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state, new ResolveIncident(incidentId, body.Resolution, agentId, Now)), ct)
);

customersIncidents.MapPost("{incidentId:guid}/acknowledge",
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

agentIncidents.MapPost("{incidentId:guid}/close",
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

app.UseSwagger()
    .UseSwaggerUI()
    .UseForwardedHeaders(); // Header forwarding to enable Swagger in Nginx


app.UseCors("ClientPermission");
app.MapHub<IncidentsHub>("/hubs/incidents");

return await app.RunOaktonCommands(args);

public class IncidentsHub: Hub;

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

public partial class Program;
