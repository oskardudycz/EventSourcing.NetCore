using System.Net;
using Carts;
using Core;
using Core.EventStoreDB;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.Exceptions;
using Core.OpenTelemetry;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using EventStore.Client;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carts", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddCoreServices()
    .AddEventStoreDBSubscriptionToAll()
    .AddCartsModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware(
        sp => sp.GetRequiredService<EventStoreDBExpectedStreamRevisionProvider>().TrySet,
        sp => () => sp.GetRequiredService<EventStoreDBNextStreamRevisionProvider>().Value?.ToString()
    )
    .AddOpenTelemetry("Carts", OpenTelemetryOptions.Build(options =>
        options.Configure(t =>
            t.AddJaegerExporter()
        ).DisableConsoleExporter(true)
    ))
    .AddControllers();

var app = builder.Build();

app.UseExceptionHandlingMiddleware(exception => exception switch
    {
        AggregateNotFoundException _ => HttpStatusCode.NotFound,
        WrongExpectedVersionException => HttpStatusCode.PreconditionFailed,
        _ => HttpStatusCode.InternalServerError
    })
    .UseOptimisticConcurrencyMiddleware()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    })
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carts V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program
{
}
