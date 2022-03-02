using System;
using Core.Events;

namespace ECommerce.ShoppingCarts.GettingCarts;

public record ShoppingCartShortInfo
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public int TotalItemsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public int Version { get; set; }

    public ulong LastProcessedPosition { get; set; }
}

public class ShoppingCartShortInfoProjection
{
    public static ShoppingCartShortInfo Handle(StreamEvent<ShoppingCartInitialized> @event)
    {
        var (shoppingCartId, clientId) = @event.Data;

        return new ShoppingCartShortInfo
        {
            Id = shoppingCartId,
            ClientId = clientId,
            TotalItemsCount = 0,
            Status = ShoppingCartStatus.Pending,
            Version = 0,
            LastProcessedPosition = @event.Metadata.LogPosition
        };
    }

    public static void Handle(StreamEvent<ShoppingCartConfirmed> @event, ShoppingCartShortInfo view)
    {
        if (view.LastProcessedPosition >= @event.Metadata.LogPosition)
            return;

        view.Status = ShoppingCartStatus.Confirmed;
        view.Version++;
        view.LastProcessedPosition = @event.Metadata.LogPosition;
    }

    public static void Handle(StreamEvent<ProductItemAddedToShoppingCart> @event, ShoppingCartShortInfo view)
    {
        if (view.LastProcessedPosition >= @event.Metadata.LogPosition)
            return;

        var productItem = @event.Data.ProductItem;

        view.TotalItemsCount += productItem.Quantity;
        view.TotalPrice += productItem.TotalPrice;
        view.Version++;
        view.LastProcessedPosition = @event.Metadata.LogPosition;
    }

    public static void Handle(StreamEvent<ProductItemRemovedFromShoppingCart> @event, ShoppingCartShortInfo view)
    {
        if (view.LastProcessedPosition >= @event.Metadata.LogPosition)
            return;

        var productItem = @event.Data.ProductItem;

        view.TotalItemsCount -= productItem.Quantity;
        view.TotalPrice -= productItem.TotalPrice;
        view.Version++;
        view.LastProcessedPosition = @event.Metadata.LogPosition;
    }
}
