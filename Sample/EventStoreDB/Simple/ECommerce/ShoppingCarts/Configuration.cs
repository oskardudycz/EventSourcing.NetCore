using Core.Events;
using ECommerce.Core.Commands;
using ECommerce.Core.Entities;
using ECommerce.ShoppingCarts.ConfirmingCart;
using ECommerce.ShoppingCarts.Initializing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ShoppingCarts
{
    public static class Configuration
    {
        public static IServiceCollection AddShoppingCartsModule(this IServiceCollection services)
            => services
                .AddTransient<IEventStoreDBRepository<ShoppingCart>, EventStoreDBRepository<ShoppingCart>>()
                .AddCreateCommandHandler<ShoppingCart, InitializeShoppingCart>(
                    InitializeShoppingCart.Handle,
                    command => StreamNameMapper.ToStreamId<ShoppingCart>(command.ShoppingCartId)
                )
                .AddUpdateCommandHandler<ShoppingCart, ConfirmCart>(
                    ConfirmCart.Handle,
                    command => StreamNameMapper.ToStreamId<ShoppingCart>(command.ShoppingCartId),
                    ShoppingCart.When
                );
    }
}
