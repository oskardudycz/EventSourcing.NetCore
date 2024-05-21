using Carts;
using Core;
using Core.Events;
using Core.EventStoreDB;
using Core.EventStoreDB.Subscriptions;
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
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            AggregateNotFoundException => exception.MapToProblemDetails(StatusCodes.Status404NotFound),
            WrongExpectedVersionException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null
        })
    .AddEventStoreDBSubscriptionToAll<EventBusBatchHandler>("DDDESDB")
    .AddCartsModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware()
    .AddOpenTelemetry("Carts", OpenTelemetryOptions.Build(options =>
        options.WithTracing(t =>
            t.AddJaegerExporter()
        ).DisableConsoleExporter(true)
    ))
    .AddControllers();

var app = builder.Build();

app.UseExceptionHandler()
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

public partial class Program;
