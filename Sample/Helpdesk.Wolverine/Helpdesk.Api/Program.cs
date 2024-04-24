using System.Text.Json.Serialization;
using Helpdesk.Api;
using Helpdesk.Api.Core.Http.Middlewares.ExceptionHandling;
using Helpdesk.Api.Core.Kafka;
using Helpdesk.Api.Core.SignalR;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GettingCustomerIncidents;
using Helpdesk.Api.Incidents.GettingCustomerIncidentsSummary;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.GettingHistory;
using Helpdesk.Api.Incidents.ResolutionBatch;
using JasperFx.CodeGeneration;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Exceptions;
using Marten.Services.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Oakton;
using Oakton.Resources;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            ConcurrencyException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null
        })
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

        options.UseSystemTextJsonForSerialization(
            EnumStorage.AsString,
            casing: Casing.CamelCase
        );

        options.Projections.Add(new KafkaProducer(builder.Configuration), ProjectionLifecycle.Async);
        options.Projections.Add(
            new SignalRProducer((IHubContext)sp.GetRequiredService<IHubContext<IncidentsHub>>()),
            ProjectionLifecycle.Async
        );

        options.ConfigureIncidents();

        return options;
    })
    .OptimizeArtifactWorkflow(TypeLoadMode.Static)
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.Solo)
    // Add Marten/PostgreSQL integration with Wolverine's outbox
    .IntegrateWithWolverine()
    // I also added this to opt into events being forward to
    // the Wolverine outbox during SaveChangesAsync()
    .EventForwardingToWolverine();

builder.Services.AddResourceSetupOnStartup();

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
// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    opts.ConfigureIncidents();
    opts.Policies.AutoApplyTransactions();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

app.UseExceptionHandler()
    .UseCors("ClientPermission");

app.MapHub<IncidentsHub>("/hubs/incidents");

// Let's add in Wolverine HTTP endpoints to the routing tree
app.MapWolverineEndpoints();

return await app.RunOaktonCommands(args);

namespace Helpdesk.Api
{
    public class IncidentsHub: Hub
    {
    }

    public partial class Program
    {
    }
}
