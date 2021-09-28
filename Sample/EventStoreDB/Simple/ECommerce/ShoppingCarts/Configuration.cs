using System.Collections.Generic;
using ECommerce.Core.Commands;
using ECommerce.Core.Entities;
using ECommerce.Core.Events;
using ECommerce.Core.Projections;
using ECommerce.Core.Queries;
using ECommerce.ShoppingCarts.Confirming;
using ECommerce.ShoppingCarts.GettingCartById;
using ECommerce.ShoppingCarts.GettingCarts;
using ECommerce.ShoppingCarts.Initializing;
using ECommerce.Storage;
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
                )
                .AddProjection<ShoppingCartDetailsProjection>(
                    typeof(ShoppingCartInitialized),
                    typeof(ShoppingCartConfirmed)
                )
                .AddEntityFrameworkQueryHandler<ECommerceDBContext, GetCartById, ShoppingCartDetails>(
                    GetCartById.Handle
                )
                .AddProjection<ShoppingCartShortInfoProjection>(
                    typeof(ShoppingCartInitialized),
                    typeof(ShoppingCartConfirmed)
                )
                .AddEntityFrameworkQueryHandler<ECommerceDBContext, GetCarts, ShoppingCartShortInfo>(
                    GetCarts.Handle
                );

        public static void SetupShoppingCartsReadModels(this ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ShoppingCartShortInfo>();

            modelBuilder
                .Entity<ShoppingCartDetails>();
        }
    }
}
