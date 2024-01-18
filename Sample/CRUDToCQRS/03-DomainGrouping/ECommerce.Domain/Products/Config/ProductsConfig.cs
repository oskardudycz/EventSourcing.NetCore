using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Products.Repositories;
using ECommerce.Domain.Products.Services;
using ECommerce.Domain.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Domain.Products.Config;

internal static class ProductsConfig
{
    internal static IServiceCollection AddProducts(this IServiceCollection services) =>
        services
            .AddTransient(sp => sp.GetRequiredService<ECommerceDbContext>().Set<Product>())
            .AddTransient(sp => sp.GetRequiredService<ECommerceDbContext>().Set<Product>().AsNoTracking())
            .AddScoped<ProductReadOnlyRepository>()
            .AddScoped<ProductRepository>()
            .AddScoped<ProductService>()
            .AddScoped<ProductReadOnlyService>();
}
