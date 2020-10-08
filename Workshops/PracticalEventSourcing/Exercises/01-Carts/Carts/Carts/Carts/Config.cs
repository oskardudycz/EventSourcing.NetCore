using Carts.Carts.Commands;
using Carts.Carts.Events;
using Carts.Carts.Queries;
using Carts.Pricing;
using Core.Repositories;
using Core.Storage;
using Marten;
using Marten.Pagination;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Carts.Carts
{
    internal static class CartsConfig
    {
        internal static void AddCarts(this IServiceCollection services)
        {
            services.AddScoped<IProductPriceCalculator, RandomProductPriceCalculator>();



            AddCommandHandlers(services);
            AddQueryHandlers(services);
            AddEventHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
        }

        internal static void ConfigureCarts(this StoreOptions options)
        {
        }
    }
}
