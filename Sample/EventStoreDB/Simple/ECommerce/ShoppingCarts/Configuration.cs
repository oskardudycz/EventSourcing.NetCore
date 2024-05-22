using Core.EntityFramework.Projections;
using ECommerce.Core.Commands;
using ECommerce.Pricing.ProductPricing;
using ECommerce.ShoppingCarts.AddingProductItem;
using ECommerce.ShoppingCarts.Canceling;
using ECommerce.ShoppingCarts.Confirming;
using ECommerce.ShoppingCarts.GettingCartById;
using ECommerce.ShoppingCarts.GettingCarts;
using ECommerce.ShoppingCarts.Opening;
using ECommerce.ShoppingCarts.RemovingProductItem;
using ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ShoppingCarts;

public static class Configuration
{
    public static IServiceCollection AddShoppingCartsModule(this IServiceCollection services) =>
        services
            .For<ShoppingCart>(
                ShoppingCart.Default,
                ShoppingCart.Evolve,
                builder => builder
                    .AddOn<OpenShoppingCart>(
                        OpenShoppingCart.Handle,
                        command => ShoppingCart.MapToStreamId(command.ShoppingCartId)
                    )
                    .UpdateOn<AddProductItemToShoppingCart>(
                        sp =>
                            (command, shoppingCart) =>
                                AddProductItemToShoppingCart.Handle(
                                    sp.GetRequiredService<IProductPriceCalculator>(),
                                    command,
                                    shoppingCart
                                ),
                        command => ShoppingCart.MapToStreamId(command.ShoppingCartId)
                    )
                    .UpdateOn<RemoveProductItemFromShoppingCart>(
                        RemoveProductItemFromShoppingCart.Handle,
                        command => ShoppingCart.MapToStreamId(command.ShoppingCartId)
                    )
                    .UpdateOn<ConfirmShoppingCart>(
                        ConfirmShoppingCart.Handle,
                        command => ShoppingCart.MapToStreamId(command.ShoppingCartId)
                    )
                    .UpdateOn<CancelShoppingCart>(
                        CancelShoppingCart.Handle,
                        command => ShoppingCart.MapToStreamId(command.ShoppingCartId)
                    )
            )
            .For<ShoppingCartDetails, Guid, ECommerceDbContext>(
                builder => builder
                    .ViewId(v => v.Id)
                    .AddOn<ShoppingCartOpened>(ShoppingCartDetailsProjection.Handle)
                    .UpdateOn<ProductItemAddedToShoppingCart>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle
                    )
                    .UpdateOn<ProductItemRemovedFromShoppingCart>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle
                    )
                    .UpdateOn<ShoppingCartConfirmed>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle
                    )
                    .UpdateOn<ShoppingCartCanceled>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle
                    )
                    .Include(x => x.ProductItems)
                    .QueryWith<GetCartById, ShoppingCartDetails?>(GetCartById.Handle)
            )
            .For<ShoppingCartShortInfo, Guid, ECommerceDbContext>(
                builder => builder
                    .ViewId(v => v.Id)
                    .AddOn<ShoppingCartOpened>(ShoppingCartShortInfoProjection.Handle)
                    .UpdateOn<ProductItemAddedToShoppingCart>(
                        e => e.ShoppingCartId,
                        ShoppingCartShortInfoProjection.Handle
                    )
                    .UpdateOn<ProductItemRemovedFromShoppingCart>(
                        e => e.ShoppingCartId,
                        ShoppingCartShortInfoProjection.Handle
                    )
                    .UpdateOn<ShoppingCartConfirmed>(
                        e => e.ShoppingCartId,
                        ShoppingCartShortInfoProjection.Handle
                    )
                    .UpdateOn<ShoppingCartCanceled>(
                        e => e.ShoppingCartId,
                        ShoppingCartShortInfoProjection.Handle
                    )
                    .QueryWith<GetCarts>(GetCarts.Handle)
            );

    public static void SetupShoppingCartsReadModels(this ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ShoppingCartShortInfo>();

        modelBuilder
            .Entity<ShoppingCartDetails>()
            .OwnsMany(e => e.ProductItems, a =>
            {
                a.WithOwner().HasForeignKey("ShoppingCardId");
                a.Property<int>("Id").ValueGeneratedOnAdd();
                a.HasKey("Id");
            });
    }
}
