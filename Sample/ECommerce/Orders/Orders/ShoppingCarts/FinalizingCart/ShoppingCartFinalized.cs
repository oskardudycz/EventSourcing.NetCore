using Core.Events;
using Orders.Products;

namespace Orders.ShoppingCarts.FinalizingCart;

public record CartFinalized(
    Guid CartId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice,
    DateTime FinalizedAt
): IExternalEvent;
