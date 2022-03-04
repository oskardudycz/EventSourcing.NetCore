using System.Net;
using Core;
using Core.Exceptions;
using Core.Serialization.Newtonsoft;
using Core.Kafka;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.Swagger;
using Core.WebApi.Tracing;
using Microsoft.OpenApi.Models;

namespace Shipments.Api;

public class Startup
{
    private readonly IConfiguration config;

    public Startup(IConfiguration config)
    {
        this.config = config;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc()
            .AddNewtonsoftJson(opt => opt.SerializerSettings.WithDefaults());

        services.AddControllers();

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shipments", Version = "v1" });
                c.OperationFilter<MetadataOperationFilter>();
            })
            .AddKafkaProducer()
            .AddCoreServices()
            .AddShipmentsModule(config)
            .AddCorrelationIdMiddleware();
        // TODO: Add optimistic concurrency here
        // .AddOptimisticConcurrencyMiddleware();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandlingMiddleware(exception => exception switch
            {
                AggregateNotFoundException _ => HttpStatusCode.NotFound,
                // TODO: Add here EF concurrency exception
                // ConcurrencyException => HttpStatusCode.PreconditionFailed,
                _ => HttpStatusCode.InternalServerError
            })
            .UseCorrelationIdMiddleware()
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
    }
}
