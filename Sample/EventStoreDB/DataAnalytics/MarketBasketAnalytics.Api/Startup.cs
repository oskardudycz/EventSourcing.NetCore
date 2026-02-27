using Core;
using Core.ElasticSearch;
using Core.EventStoreDB;
using Core.WebApi.Middlewares.ExceptionHandling;
using Microsoft.OpenApi;

namespace MarketBasketAnalytics.Api
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce.Api", Version = "v1" });
                })
                .AddDefaultExceptionHandler()
                .AddEventStoreDB(Configuration)
                .AddElasticsearch(Configuration)
                .AddCoreServices()
                .AddMarketBasketAnalytics(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler()
                .UseSwagger()
                .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce.Api v1"))
                // .UseMiddleware(typeof(ExceptionHandlingMiddleware))
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // if (env.IsDevelopment())
            // {
            //     app.ApplicationServices.UseMarketBasketAnalytics();
            // }
        }
    }
}
