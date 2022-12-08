using Core.Commands;
using Core.Marten.Repository;

namespace Orders.Orders.CancellingOrder;

public record CancelOrder(
    Guid OrderId,
    OrderCancellationReason CancellationReason
)
{
    public static CancelOrder Create(Guid? orderId, OrderCancellationReason? cancellationReason)
    {
        if (!orderId.HasValue)
            throw new ArgumentNullException(nameof(orderId));

        if (!cancellationReason.HasValue)
            throw new ArgumentNullException(nameof(cancellationReason));

        return new CancelOrder(orderId.Value, cancellationReason.Value);
    }
}

public class HandleCancelOrder:
    ICommandHandler<CancelOrder>
{
    private readonly IMartenRepository<Order> orderRepository;

    public HandleCancelOrder(IMartenRepository<Order> orderRepository) =>
        this.orderRepository = orderRepository;

    public Task Handle(CancelOrder command, CancellationToken ct) =>
        orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Cancel(command.CancellationReason),
            ct: ct
        );
}
