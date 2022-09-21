using ECommerce.Domain.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Domain.Products;

internal static class ProductsConfig
{
    internal static IServiceCollection AddProducts(this IServiceCollection services) =>
        services
            .AddTransient(sp => sp.GetRequiredService<ECommerceDbContext>().Set<Product>())
            .AddTransient(sp => sp.GetRequiredService<ECommerceDbContext>().Set<Product>().AsNoTracking());
}
