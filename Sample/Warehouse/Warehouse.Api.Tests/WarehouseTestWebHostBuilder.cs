using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Storage;

namespace Warehouse.Api.Tests
{
    public static class WarehouseTestWebHostBuilder
    {
        public static IWebHostBuilder Configure(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .ConfigureServices(services =>
                {
                    services.AddMvcCore()
                        .AddAuthorization()
                        .AddCors()
                        .AddDataAnnotations()
                        .AddFormatterMappings();

                    services.AddWarehouseServices();
                })
                .Configure(app =>
                {
                    app.UseHttpsRedirection()
                        .UseRouting()
                        .UseAuthorization()
                        .UseEndpoints(endpoints => { endpoints.UseWarehouseEndpoints(); });

                    // Kids, do not try this at home!
                    app.ApplicationServices.GetRequiredService<WarehouseDBContext>().Database.Migrate();
                });

            return webHostBuilder;
        }
    }
}
