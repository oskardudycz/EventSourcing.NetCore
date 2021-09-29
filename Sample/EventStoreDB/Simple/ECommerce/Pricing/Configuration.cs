using ECommerce.Pricing.ProductPricing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Pricing
{
    public static class Configuration
    {
        public static IServiceCollection AddPricingModule(this IServiceCollection services)
            => services.AddSingleton<IProductPriceCalculator, RandomProductPriceCalculator>();
    }
}
