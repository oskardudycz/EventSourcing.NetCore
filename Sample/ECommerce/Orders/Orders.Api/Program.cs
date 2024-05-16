using System.Net;
using Confluent.Kafka;
using Core;
using Core.Configuration;
using Core.Exceptions;
using Core.Kafka;
using Core.OpenTelemetry;
using Core.Scheduling;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using Marten.Exceptions;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Orders;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.AddKafkaConsumer<string, string>("kafka", settings =>
{
    settings.Config.GroupId = "Orders";
    settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
    settings.Config.EnableAutoCommit = false;
    settings.Config.AllowAutoCreateTopics = true;
});

builder.AddKafkaProducer<string, string>("kafka", settings =>
{
    settings.Config.AllowAutoCreateTopics = true;
});

builder
    .AddServiceDefaults()
    .Services
    .AddNpgsqlDataSource(builder.Configuration.GetRequiredConnectionString("orders"))
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaProducerAndConsumer()
    .AddCoreServices()
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            AggregateNotFoundException => exception.MapToProblemDetails(StatusCodes.Status404NotFound),
            ConcurrencyException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null
        })
    .AddOrdersModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware()
    .AddOpenTelemetry("Orders", OpenTelemetryOptions.Build(options =>
        options
            .WithTracing(t => t.AddSource("Marten"))
            .WithMetrics(m => m.AddMeter("Marten"))
            .DisableConsoleExporter(true)
    ))
    .AddQuartzDefaults()
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program
{
}
