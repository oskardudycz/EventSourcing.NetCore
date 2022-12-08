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

public class HandleInitializeOrder:
    ICommandHandler<InitializeOrder>
{
    private readonly IMartenRepository<Order> orderRepository;

    public HandleInitializeOrder(IMartenRepository<Order> orderRepository) =>
        this.orderRepository = orderRepository;

    public Task Handle(InitializeOrder command, CancellationToken cancellationToken)
    {
        var (orderId, clientId, productItems, totalPrice) = command;

        return orderRepository.Add(
            Order.Initialize(orderId, clientId, productItems, totalPrice),
            cancellationToken
        );
    }
}
