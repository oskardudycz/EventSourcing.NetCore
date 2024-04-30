using ECommerce.Core.Commands;
using ECommerce.Core.Projections;
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
            .For<ShoppingCartDetails, ECommerceDbContext>(
                builder => builder
                    .AddOn<ShoppingCartOpened>(ShoppingCartDetailsProjection.Handle)
                    .UpdateOn<ProductItemAddedToShoppingCart>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle,
                        (entry, ct) => entry.Collection(x => x.ProductItems).LoadAsync(ct)
                    )
                    .UpdateOn<ProductItemRemovedFromShoppingCart>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle,
                        (entry, ct) => entry.Collection(x => x.ProductItems).LoadAsync(ct)
                    )
                    .UpdateOn<ShoppingCartConfirmed>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle
                    )
                    .UpdateOn<ShoppingCartCanceled>(
                        e => e.ShoppingCartId,
                        ShoppingCartDetailsProjection.Handle
                    )
                    .QueryWith<GetCartById>(GetCartById.Handle)
            )
            .For<ShoppingCartShortInfo, ECommerceDbContext>(
                builder => builder
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
