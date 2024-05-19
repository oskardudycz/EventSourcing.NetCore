using System.Text.Json.Serialization;
using Helpdesk.Api.Core.Http.Middlewares.ExceptionHandling;
using Helpdesk.Api.Core.Kafka;
using Helpdesk.Api.Core.SignalR;
using Helpdesk.Api.Incidents;
using JasperFx.CodeGeneration;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Exceptions;
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

        var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "WolverineHelpdesk";
        options.Events.DatabaseSchemaName = schemaName;
        options.DatabaseSchemaName = schemaName;
        options.Connection(builder.Configuration.GetConnectionString("WolverineIncidents") ??
                           throw new InvalidOperationException());

        options.UseSystemTextJsonForSerialization(
            EnumStorage.AsString,
            casing: Casing.CamelCase
        );

        options.Projections.Errors.SkipApplyErrors = false;
        options.Projections.Errors.SkipSerializationErrors = false;
        options.Projections.Errors.SkipUnknownEvents = false;

        options.Projections.RebuildErrors.SkipApplyErrors = false;
        options.Projections.RebuildErrors.SkipSerializationErrors = false;
        options.Projections.RebuildErrors.SkipUnknownEvents = false;

        options.ConfigureIncidents();

        options.DisableNpgsqlLogging = true;
        return options;
    })
    .OptimizeArtifactWorkflow(TypeLoadMode.Static)
    .UseLightweightSessions()
    .AddSubscriptionWithServices<KafkaProducer>(ServiceLifetime.Singleton)
    .AddSubscriptionWithServices<SignalRProducer<IncidentsHub>>(ServiceLifetime.Singleton)
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

public class IncidentsHub: Hub
{
}

public partial class Program
{
}
