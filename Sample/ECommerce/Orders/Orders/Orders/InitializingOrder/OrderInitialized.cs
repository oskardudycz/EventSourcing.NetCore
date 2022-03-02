using System;
using System.Collections.Generic;
using Core.Events;
using Orders.Products;

namespace Orders.Orders.InitializingOrder;

public record OrderInitialized(
    Guid OrderId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice,
    DateTime InitializedAt
): IEvent
{
    public static OrderInitialized Create(
        Guid orderId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTime initializedAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));
        if (productItems.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(productItems.Count));
        if (totalPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalPrice));
        if (initializedAt == default)
            throw new ArgumentOutOfRangeException(nameof(initializedAt));

        return new OrderInitialized(orderId, clientId, productItems, totalPrice, initializedAt);
    }
}
