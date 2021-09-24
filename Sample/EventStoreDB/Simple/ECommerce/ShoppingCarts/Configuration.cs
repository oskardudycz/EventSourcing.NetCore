using ECommerce.Core.Commands;
using ECommerce.Core.Entities;
using ECommerce.ShoppingCarts.Initializing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ShoppingCarts
{
    public static class Configuration
    {
        public static IServiceCollection AddShoppingCartsModule(this IServiceCollection services)
            => services
                .AddTransient<IEventStoreDBRepository<ShoppingCart>, EventStoreDBRepository<ShoppingCart>>()
                .AddCommandHandler<InitializeShoppingCart, HandleInitializeShoppingCart>();
    }
}
