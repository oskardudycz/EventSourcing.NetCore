using System.Net;
using Core;
using Core.Exceptions;
using Core.Kafka;
using Core.Marten.OptimisticConcurrency;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using Marten.Exceptions;
using Microsoft.OpenApi.Models;
using Payments;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payments", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaProducer()
    .AddCoreServices()
    .AddPaymentsModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware(
        sp => sp.GetRequiredService<MartenExpectedStreamVersionProvider>().TrySet,
        sp => () => sp.GetRequiredService<MartenNextStreamVersionProvider>().Value?.ToString()
    )
    .AddControllers();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program
{
}
