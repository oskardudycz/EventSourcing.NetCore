using Core.Commands;
using Core.Marten.Repository;

namespace Orders.Orders.CompletingOrder;

public record CompleteOrder(
    Guid OrderId
)
{
    public static CompleteOrder Create(Guid? orderId)
    {
        if (orderId == null || orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));

        return new CompleteOrder(orderId.Value);
    }
}

public class HandleCompleteOrder(IMartenRepository<Order> orderRepository):
    ICommandHandler<CompleteOrder>
{
    public Task Handle(CompleteOrder command, CancellationToken ct) =>
        orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Complete(),
            ct: ct
        );
}
