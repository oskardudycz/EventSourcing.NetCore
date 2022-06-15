using System.Net;
using Core;
using Core.Exceptions;
using Core.Kafka;
using Core.Marten.OptimisticConcurrency;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using Core.WebApi.Tracing;
using Marten.Exceptions;
using MeetingsManagement;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Meeting Management", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaProducerAndConsumer()
    .AddCoreServices()
    .AddMeetingsManagement(builder.Configuration)
    .AddCorrelationIdMiddleware()
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
    .UseCorrelationIdMiddleware()
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meeting Management V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program
{
}
