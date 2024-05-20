using Core.Events;

namespace ECommerce.ShoppingCarts.GettingCartById;

public class ShoppingCartDetails
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public List<ShoppingCartDetailsProductItem> ProductItems { get; set; } = default!;
    public int Version { get; set; }
    public ulong LastProcessedPosition { get; set; }
}

public class ShoppingCartDetailsProductItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public static class ShoppingCartDetailsProjection
{
    public static ShoppingCartDetails Handle(EventEnvelope<ShoppingCartOpened> eventEnvelope)
    {
        var (shoppingCartId, clientId) = eventEnvelope.Data;

        return new ShoppingCartDetails
        {
            Id = shoppingCartId,
            ClientId = clientId,
            Status = ShoppingCartStatus.Pending,
            Version = 0,
            LastProcessedPosition = eventEnvelope.Metadata.LogPosition
        };
    }

    public static void Handle(ShoppingCartDetails view, EventEnvelope<ProductItemAddedToShoppingCart> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        var productItem = eventEnvelope.Data.ProductItem;
        var existingProductItem = view.ProductItems
            .FirstOrDefault(x => x.ProductId == productItem.ProductId);

        if (existingProductItem == null)
        {
            view.ProductItems.Add(new ShoppingCartDetailsProductItem
            {
                ProductId = productItem.ProductId,
                Quantity = productItem.Quantity,
                UnitPrice = productItem.UnitPrice
            });
        }
        else
        {
            existingProductItem.Quantity += productItem.Quantity;
        }

        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }

    public static void Handle(ShoppingCartDetails view, EventEnvelope<ProductItemRemovedFromShoppingCart> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        var productItem = eventEnvelope.Data.ProductItem;
        var existingProductItem = view.ProductItems
            .Single(x => x.ProductId == productItem.ProductId);

        if (existingProductItem.Quantity == productItem.Quantity)
        {
            view.ProductItems.Remove(existingProductItem);
        }
        else
        {
            existingProductItem.Quantity -= productItem.Quantity;
        }

        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }

    public static void Handle(ShoppingCartDetails view, EventEnvelope<ShoppingCartConfirmed> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        view.Status = ShoppingCartStatus.Confirmed;
        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }

    public static void Handle(ShoppingCartDetails view, EventEnvelope<ShoppingCartCanceled> eventEnvelope)
    {
        if (view.LastProcessedPosition >= eventEnvelope.Metadata.LogPosition)
            return;

        view.Status = ShoppingCartStatus.Canceled;
        view.Version++;
        view.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
    }
}
