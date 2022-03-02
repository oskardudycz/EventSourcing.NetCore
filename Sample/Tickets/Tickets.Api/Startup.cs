using System.Net;
using Core;
using Core.Exceptions;
using Core.Marten.OptimisticConcurrency;
using Core.Serialization.Newtonsoft;
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

namespace Tickets.Api;

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tickets", Version = "v1" });
                c.OperationFilter<MetadataOperationFilter>();
            })
            .AddCoreServices()
            .AddTicketsModule(config)
            .AddCorrelationIdMiddleware()
            .AddOptimisticConcurrencyMiddleware(
                sp => sp.GetRequiredService<MartenExpectedStreamVersionProvider>().TrySet,
                sp => () => sp.GetRequiredService<MartenNextStreamVersionProvider>().Value?.ToString()
            );
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tickets V1");
                c.RoutePrefix = string.Empty;
            });
    }
}
