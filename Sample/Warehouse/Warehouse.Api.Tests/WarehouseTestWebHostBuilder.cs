using Core.WebApi.Middlewares.ExceptionHandling;
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
                    services.AddRouting()
                        .AddAuthorization()
                        .AddCors()
                        .AddWarehouseServices();
                })
                .Configure(app =>
                {
                    app.UseHttpsRedirection()
                        .UseMiddleware(typeof(ExceptionHandlingMiddleware))
                        .UseRouting()
                        .UseAuthorization()
                        .UseEndpoints(endpoints => { endpoints.UseWarehouseEndpoints(); });

                    // Kids, do not try this at home!
                    var database = app.ApplicationServices.GetRequiredService<WarehouseDBContext>().Database;
                    database.Migrate();
                    database.ExecuteSqlRaw("TRUNCATE TABLE \"Product\"");
                });

            return webHostBuilder;
        }
    }
}
