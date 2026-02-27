using Core;
using Core.Exceptions;
using Core.OpenTelemetry;
using Core.Serialization.Newtonsoft;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using JasperFx;
using Microsoft.OpenApi;
using Npgsql;
using OpenTelemetry.Trace;
using Tickets;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tickets", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddCoreServices()
    .AddDefaultExceptionHandler((exception, _) => exception switch
    {
        AggregateNotFoundException => exception.MapToProblemDetails(StatusCodes.Status404NotFound),
        ConcurrencyException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
        _ => null
    })
    .AddTicketsModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware()
    .AddOpenTelemetry("Tickets", OpenTelemetryOptions.Build(options =>
        options.WithTracing(t =>
            t.AddJaegerExporter()
                .AddNpgsql()
        ).DisableConsoleExporter(true)
    ))
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
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Carts V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program;
