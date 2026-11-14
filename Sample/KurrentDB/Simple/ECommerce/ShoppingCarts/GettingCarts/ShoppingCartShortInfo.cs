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
    public static ShoppingCartShortInfo Handle(EventEnvelope<ShoppingCartOpened> eventEnvelope)
    {
        var (shoppingCartId, clientId) = eventEnvelope.Data;

        return new ShoppingCartShortInfo
        {
            Id = shoppingCartId,
            ClientId = clientId,
            TotalItemsCount = 0,
            Status = ShoppingCartStatus.Pending,
            Version = 0,
            LastProcessedPosition = eventEnvelope.Metadata.LogPosition
        };
    }

    public static void Handle(ShoppingCartShortInfo view, EventEnvelope<ProductItemAddedToShoppingCart> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        var productItem = eventEnvelope.Data.ProductItem;

        view.TotalItemsCount += productItem.Quantity;
        view.TotalPrice += productItem.TotalPrice;
        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }

    public static void Handle(ShoppingCartShortInfo view, EventEnvelope<ProductItemRemovedFromShoppingCart> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        var productItem = eventEnvelope.Data.ProductItem;

        view.TotalItemsCount -= productItem.Quantity;
        view.TotalPrice -= productItem.TotalPrice;
        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }

    public static void Handle(ShoppingCartShortInfo view, EventEnvelope<ShoppingCartConfirmed> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        view.Status = ShoppingCartStatus.Confirmed;
        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }

    public static void Handle(ShoppingCartShortInfo view, EventEnvelope<ShoppingCartCanceled> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        view.Status = ShoppingCartStatus.Canceled;
        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }
}
