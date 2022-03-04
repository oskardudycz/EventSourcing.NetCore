using System.Net;
using Core;
using Core.EventStoreDB;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.Exceptions;
using Core.Serialization.Newtonsoft;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using Core.WebApi.Tracing;
using EventStore.Client;
using Microsoft.OpenApi.Models;

namespace Carts.Api;

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

        services
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carts", Version = "v1" });
                c.OperationFilter<MetadataOperationFilter>();
            })
            .AddCoreServices()
            .AddEventStoreDBSubscriptionToAll()
            .AddCartsModule(config)
            .AddCorrelationIdMiddleware()
            .AddOptimisticConcurrencyMiddleware(
                sp => sp.GetRequiredService<EventStoreDBExpectedStreamRevisionProvider>().TrySet,
                sp => () => sp.GetRequiredService<EventStoreDBNextStreamRevisionProvider>().Value?.ToString()
            );
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
                WrongExpectedVersionException => HttpStatusCode.PreconditionFailed,
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carts V1");
                c.RoutePrefix = string.Empty;
            });
    }
}
