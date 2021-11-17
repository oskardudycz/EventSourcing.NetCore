using System;
using System.Collections.Generic;
using Core.Events;
using Orders.Products;

namespace Orders.Orders.InitializingOrder;

public class OrderInitialized: IEvent
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }

    public IReadOnlyList<PricedProductItem> ProductItems { get; }

    public decimal TotalPrice { get; }

    public DateTime InitializedAt { get; }

    private OrderInitialized(
        Guid orderId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTime initializedAt)
    {
        OrderId = orderId;
        ClientId = clientId;
        ProductItems = productItems;
        TotalPrice = totalPrice;
        InitializedAt = initializedAt;
    }

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