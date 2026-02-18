using Core;
using Core.Exceptions;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using JasperFx;
using Microsoft.OpenApi.Models;
using SmartHome.Temperature;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Smart Home", Version = "v1" });
        c.OperationFilter<MetadataOperationFilter>();
    })
    .AddCoreServices()
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            AggregateNotFoundException => exception.MapToProblemDetails(StatusCodes.Status404NotFound),
            ConcurrencyException => exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null,
        })
    .AddTemperaturesModule(builder.Configuration)
    .AddOptimisticConcurrencyMiddleware();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Home V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program;
