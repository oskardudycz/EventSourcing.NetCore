using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Products;
using Warehouse.Storage;

namespace Warehouse;

public static class WarehouseConfiguration
{
    public static IServiceCollection AddWarehouseServices(this IServiceCollection services)
        => services
            .AddDbContext<WarehouseDBContext>(
                options => options.UseNpgsql("name=ConnectionStrings:WarehouseDB"))
            .AddProductServices();

    public static IEndpointRouteBuilder UseWarehouseEndpoints(this IEndpointRouteBuilder endpoints)
        => endpoints.UseProductsEndpoints();

    public static IApplicationBuilder ConfigureWarehouse(this IApplicationBuilder app)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        if (environment == "Development")
        {
            app.ApplicationServices.GetRequiredService<WarehouseDBContext>().Database.Migrate();
        }

        return app;
    }
}
