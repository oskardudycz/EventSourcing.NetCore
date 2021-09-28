using ECommerce.Core.Commands;
using ECommerce.Core.Entities;
using ECommerce.ShoppingCarts.Confirming;
using ECommerce.ShoppingCarts.Initializing;
using Microsoft.EntityFrameworkCore;
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
                    command => ShoppingCart.MapToStreamId(command.ShoppingCartId)
                )
                .AddUpdateCommandHandler<ShoppingCart, ConfirmShoppingCart>(
                    ConfirmShoppingCart.Handle,
                    command => ShoppingCart.MapToStreamId(command.ShoppingCartId),
                    ShoppingCart.When
                );

        public static void SetupShoppingCartsReadModels(this ModelBuilder modelBuilder) {}
            // => modelBuilder.Entity<Product>()
            //     .OwnsOne(p => p.Sku);
    }
}
