using ECommerce.Domain.Products;
using ECommerce.Domain.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Domain;

public static class ECommerceConfig
{
    public static IServiceCollection AddECommerce(this IServiceCollection services) =>
        services
            .AddDbContext<ECommerceDbContext>()
            .AddProducts();
}
