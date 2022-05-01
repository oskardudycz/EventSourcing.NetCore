using Core;
using Core.Kafka;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.Swagger;
using Core.WebApi.Tracing;
using MeetingsSearch;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Meeting Search", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaConsumer()
    .AddCoreServices()
    .AddMeetingsSearch(builder.Configuration)
    .AddCorrelationIdMiddleware();

var app = builder.Build();

app.UseExceptionHandlingMiddleware()
    .UseCorrelationIdMiddleware()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    })
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meeting Search V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program
{
}
