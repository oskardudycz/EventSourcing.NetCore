using System.Net;
using Carts;
using Core;
using Core.Exceptions;
using Core.Kafka;
using Core.Marten.OptimisticConcurrency;
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

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carts", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaProducer()
    .AddCoreServices()
    .AddCartsModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware(
        sp => sp.GetRequiredService<MartenExpectedStreamVersionProvider>().TrySet,
        sp => () => sp.GetRequiredService<MartenNextStreamVersionProvider>().Value?.ToString()
    )
    .AddOpenTelemetry("Carts", OpenTelemetryOptions.Build(options =>
        options.Configure(t =>
            t.AddJaegerExporter()
                .AddNpgsql()
        ).DisableConsoleExporter(true)
    ))
    .AddControllers()
    .AddNewtonsoftJson(opt => opt.SerializerSettings.WithDefaults());

var app = builder.Build();

app.UseExceptionHandlingMiddleware(exception => exception switch
    {
        AggregateNotFoundException _ => HttpStatusCode.NotFound,
        ConcurrencyException => HttpStatusCode.PreconditionFailed,
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
