using ECommerce.Domain.Products;
using ECommerce.Domain.Products.CreatingProduct;
using ECommerce.Domain.Products.DeletingProduct;
using ECommerce.Domain.Products.GettingById;
using ECommerce.Domain.Products.GettingProducts;
using ECommerce.Domain.Products.UpdatingProduct;
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
