using Core.Commands;
using Core.Marten.Repository;
using Orders.Products;

namespace Orders.Orders.InitializingOrder;

public record InitializeOrder(
    Guid OrderId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice
)
{
    public static InitializeOrder Create(
        Guid? orderId,
        Guid? clientId,
        IReadOnlyList<PricedProductItem>? productItems,
        decimal? totalPrice
    )
    {
        if (!orderId.HasValue)
            throw new ArgumentNullException(nameof(orderId));
        if (!clientId.HasValue)
            throw new ArgumentNullException(nameof(clientId));
        if (productItems == null)
            throw new ArgumentNullException(nameof(productItems));
        if (!totalPrice.HasValue)
            throw new ArgumentNullException(nameof(totalPrice));

        return new InitializeOrder(orderId.Value, clientId.Value, productItems, totalPrice.Value);
    }
}

public class HandleInitializeOrder(IMartenRepository<Order> orderRepository):
    ICommandHandler<InitializeOrder>
{
    public Task Handle(InitializeOrder command, CancellationToken ct)
    {
        var (orderId, clientId, productItems, totalPrice) = command;

        return orderRepository.Add(
            Order.Initialize(orderId, clientId, productItems, totalPrice),
            ct
        );
    }
}
