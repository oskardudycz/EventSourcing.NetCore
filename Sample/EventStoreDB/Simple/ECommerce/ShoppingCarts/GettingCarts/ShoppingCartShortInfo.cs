using System;

namespace ECommerce.ShoppingCarts.GettingCarts;

public record ShoppingCartShortInfo
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public int TotalItemsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public int Version { get; set; }
}

public class ShoppingCartShortInfoProjection
{
    public static ShoppingCartShortInfo Handle(ShoppingCartInitialized @event)
    {
        var (shoppingCartId, clientId) = @event;

        return new ShoppingCartShortInfo
        {
            Id = shoppingCartId,
            ClientId = clientId,
            TotalItemsCount = 0,
            Status = ShoppingCartStatus.Pending,
            Version = 0
        };
    }

    public static void Handle(ShoppingCartConfirmed @event, ShoppingCartShortInfo view)
    {
        view.Status = ShoppingCartStatus.Confirmed;
        view.Version++;
    }

    public static void Handle(ProductItemAddedToShoppingCart @event, ShoppingCartShortInfo view)
    {
        view.TotalItemsCount += @event.ProductItem.Quantity;
        view.TotalPrice += @event.ProductItem.TotalPrice;
        view.Version++;
    }

    public static void Handle(ProductItemRemovedFromShoppingCart @event, ShoppingCartShortInfo view)
    {
        view.TotalItemsCount -= @event.ProductItem.Quantity;
        view.TotalPrice -= @event.ProductItem.TotalPrice;
        view.Version++;
    }
}