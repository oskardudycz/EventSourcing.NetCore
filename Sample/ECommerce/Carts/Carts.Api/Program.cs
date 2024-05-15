using Carts;
using Core;
using Core.Configuration;
using Core.Exceptions;
using Core.Kafka;
using Core.OpenTelemetry;
using Core.Serialization.Newtonsoft;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using Marten.Exceptions;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddKafkaProducer<string, string>("kafka", settings =>
{
    settings.Config.AllowAutoCreateTopics = true;
} );

builder
    .AddServiceDefaults()
    .Services
    .AddNpgsqlDataSource(builder.Configuration.GetRequiredConnectionString("carts"))
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carts", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaProducer()
    .AddCoreServices()
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            AggregateNotFoundException => exception.MapToProblemDetails(StatusCodes.Status404NotFound),
            ConcurrencyException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null
        })
    .AddCartsModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware()
    .AddOpenTelemetry("Carts")
    // .AddOpenTelemetry("Carts", OpenTelemetryOptions.Build(options =>
    //     options.Configure(t =>
    //         t.AddJaegerExporter()
    //             .AddNpgsql()
    //     ).DisableConsoleExporter(true)
    // ))
    .AddControllers()
    .AddNewtonsoftJson(opt => opt.SerializerSettings.WithDefaults());

var app = builder.Build();

app.UseExceptionHandler()
    .UseOptimisticConcurrencyMiddleware()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    })
    .UseExceptionHandler()
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
