using Core.Commands;
using Core.Marten.Repository;
using Core.Validation;
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
    ) =>
        new(orderId.NotEmpty(),
            clientId.NotEmpty(),
            productItems.NotNull().Has(p => p.Count.Positive()),
            totalPrice.NotEmpty().Positive()
        );
}

public class HandleInitializeOrder(IMartenRepository<Order> orderRepository, TimeProvider timeProvider):
    ICommandHandler<InitializeOrder>
{
    public Task Handle(InitializeOrder command, CancellationToken ct)
    {
        var (orderId, clientId, productItems, totalPrice) = command;

        return orderRepository.Add(
            orderId,
            Order.Initialize(orderId, clientId, productItems, totalPrice, timeProvider.GetUtcNow()),
            ct
        );
    }
}
