using ECommerce.Domain.Products;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Domain;

public static class ECommerceConfig
{
    public static IServiceCollection AddECommerce(this IServiceCollection services) =>
        services
            .AddDbContext<ECommerceDbContext>()
            .AddProducts();

    public static IEndpointRouteBuilder UseECommerceEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints
            .UseProductsEndpoints();
}
