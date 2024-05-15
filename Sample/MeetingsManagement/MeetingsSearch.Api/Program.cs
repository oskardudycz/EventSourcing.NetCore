using Core;
using Core.Kafka;
using Core.OpenTelemetry;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.Swagger;
using MeetingsSearch;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Meeting Search", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddKafkaConsumer()
    .AddCoreServices()
    .AddDefaultExceptionHandler()
    .AddMeetingsSearch(builder.Configuration)
    .AddOpenTelemetry("MeetingsSearch", OpenTelemetryOptions.Build(options =>
        options.WithTracing(t =>
            t.AddJaegerExporter()
        ).DisableConsoleExporter(true)
    ))
    .AddControllers();

var app = builder.Build();

app.UseExceptionHandler()
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
