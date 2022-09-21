using ECommerce.Domain.Products.CreatingProduct;
using ECommerce.Domain.Products.DeletingProduct;
using ECommerce.Domain.Products.GettingById;
using ECommerce.Domain.Products.GettingProducts;
using ECommerce.Domain.Products.UpdatingProduct;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Domain.Products;

internal static class ProductsConfig
{
    internal static IServiceCollection AddProducts(this IServiceCollection services) =>
        services
            .AddTransient(sp => sp.GetRequiredService<ECommerceDbContext>().Set<Product>())
            .AddTransient(sp => sp.GetRequiredService<ECommerceDbContext>().Set<Product>().AsNoTracking());


    public static IEndpointRouteBuilder UseProductsEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints
            .UseCreateProductEndpoint()
            .UseUpdateProductEndpoint()
            .UseGetProductByIdEndpoint()
            .UseGetProductsEndpoint()
            .UseDeleteProductByIdEndpoint();
}
