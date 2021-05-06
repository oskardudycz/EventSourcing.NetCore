using Microsoft.Extensions.DependencyInjection;

namespace Shipments.Products
{
    internal static class Config
    {
        internal static IServiceCollection AddProducts(this IServiceCollection services)
        {
            return services.AddScoped<IProductAvailabilityService, ProductAvailabilityService>();
        }
    }
}
