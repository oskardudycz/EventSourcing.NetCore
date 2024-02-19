using System.Text.Json.Serialization;
using Helpdesk;
using Helpdesk.Incidents.GetCustomerIncidentsSummary;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Http.Json;
using Oakton;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMartenForHelpdeskAsyncOnly(
        builder.Configuration,
        (options, _) =>
            options.Projections.Add<CustomerIncidentsSummaryProjection>(ProjectionLifecycle.Async)
    );

builder.Services
    .Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

return await app.RunOaktonCommands(args);

public partial class Program
{
}
