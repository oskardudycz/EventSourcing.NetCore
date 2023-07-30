using System.Text.Json.Serialization;
using Helpdesk;
using Helpdesk.Api.Core.Kafka;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Oakton;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMartenForHelpdeskAsyncOnly(
        builder.Configuration,
        (options, _) =>
            options.Projections.Add(new KafkaProducer(builder.Configuration), ProjectionLifecycle.Async)
    );

builder.Services
    .Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddSignalR();

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

app.UseCors("ClientPermission");
app.MapHub<IncidentsHub>("/hubs/incidents");

return await app.RunOaktonCommands(args);

public class IncidentsHub: Hub
{
}

public partial class Program
{
}
