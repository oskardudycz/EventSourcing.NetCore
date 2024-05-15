using System.Net;
using Core;
using Core.Exceptions;
using Core.Kafka;
using Core.OpenTelemetry;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.Swagger;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Trace;
using Shipments;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shipments", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaProducer()
    .AddCoreServices()
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            AggregateNotFoundException => exception.MapToProblemDetails(StatusCodes.Status404NotFound),
            // TODO: Add here EF concurrency exception
            // ConcurrencyException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null
        })
    .AddShipmentsModule(builder.Configuration)
    .AddOpenTelemetry("Shipments", OpenTelemetryOptions.Build(options =>
        options.WithTracing(t =>
            t.AddJaegerExporter()
                .AddNpgsql()
        ).DisableConsoleExporter(true)
    ))
    .AddControllers();
// TODO: Add optimistic concurrency here
// .AddOptimisticConcurrencyMiddleware();

var app = builder.Build();

app.UseExceptionHandler()
    // TODO: Add optimistic concurrency here
    // .UseOptimisticConcurrencyMiddleware()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    })
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipments V1");
        c.RoutePrefix = string.Empty;
    })
    .ConfigureShipmentsModule();

app.Run();

public partial class Program
{
}
