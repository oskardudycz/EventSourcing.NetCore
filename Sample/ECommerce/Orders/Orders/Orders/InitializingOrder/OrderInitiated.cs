using Core.Validation;
using Orders.Products;

namespace Orders.Orders.InitializingOrder;

public record OrderInitiated(
    Guid OrderId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice,
    DateTimeOffset InitiatedAt,
    DateTimeOffset TimeoutAfter
)
{
    public static OrderInitiated From(
        Guid orderId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTimeOffset initializedAt,
        DateTimeOffset timeoutAfter) =>
        new(
            orderId.NotEmpty(),
            clientId.NotEmpty(),
            productItems.Has(p => p.Count.Positive()),
            totalPrice.Positive(),
            initializedAt.NotEmpty(),
            timeoutAfter.NotEmpty()
        );
}
