using Core;
using Core.Serialization.Newtonsoft;
using Core.Kafka;
using Core.WebApi.Middlewares.ExceptionHandling;
using Core.WebApi.Swagger;
using Core.WebApi.Tracing;
using Microsoft.OpenApi.Models;

namespace MeetingsSearch.Api;

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

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Meeting Search", Version = "v1" });
            c.OperationFilter<MetadataOperationFilter>();
        });

        services
            .AddKafkaConsumer()
            .AddCoreServices()
            .AddMeetingsSearch(config)
            .AddCorrelationIdMiddleware();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

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
    }
}
