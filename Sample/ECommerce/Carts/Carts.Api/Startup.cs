using System.Net;
using System.Runtime.CompilerServices;
using Core;
using Core.Exceptions;
using Core.Marten.OptimisticConcurrency;
using Core.Serialization.Newtonsoft;
using Core.Streaming.Kafka;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.OptimisticConcurrency;
using Core.WebApi.Swagger;
using Core.WebApi.Tracing.Correlation;
using Marten.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

[assembly: InternalsVisibleTo("Marten.Generated")]

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

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carts", Version = "v1" });
            c.OperationFilter<MetadataOperationFilter>();
        });

        services
            .AddKafkaProducer()
            .AddCoreServices()
            .AddCartsModule(config)
            .AddCorrelationIdMiddleware()
            .AddOptimisticConcurrencyMiddleware(
                sp => sp.GetRequiredService<MartenExpectedStreamVersionProvider>().TrySet,
                sp => () => sp.GetRequiredService<MartenNextStreamVersionProvider>().Value?.ToString()
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Carts V1");
                c.RoutePrefix = string.Empty;
            });
    }
}
