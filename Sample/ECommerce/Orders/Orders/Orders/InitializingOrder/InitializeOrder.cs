using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;
using Orders.Products;

namespace Orders.Orders.InitializingOrder;

public record InitializeOrder(
    Guid OrderId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice
): ICommand
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
    private readonly IMartenAppendScope scope;

    public HandleInitializeOrder(
        IMartenRepository<Order> orderRepository,
        IMartenAppendScope scope
    )
    {
        this.orderRepository = orderRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(InitializeOrder command, CancellationToken cancellationToken)
    {
        var (orderId, clientId, productItems, totalPrice) = command;

        await scope.Do((_, eventMetadata) =>
            orderRepository.Add(
                Order.Initialize(orderId, clientId, productItems, totalPrice),
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
