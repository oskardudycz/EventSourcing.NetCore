using System;
using ECommerce.Core.Entities;
using ECommerce.ShoppingCarts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce
{
    public static class Configuration
    {
        public static IServiceCollection AddECommerceModule(this IServiceCollection services, IConfiguration configuration)
            => services
                .AddShoppingCartsModule()
                .AddEventStoreDB(configuration)
                .AddSingleton<Func<Guid>>(Guid.NewGuid);
    }
}
