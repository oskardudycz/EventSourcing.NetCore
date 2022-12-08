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

public class HandleCompleteOrder:
    ICommandHandler<CompleteOrder>
{
    private readonly IMartenRepository<Order> orderRepository;

    public HandleCompleteOrder(IMartenRepository<Order> orderRepository) =>
        this.orderRepository = orderRepository;

    public Task Handle(CompleteOrder command, CancellationToken cancellationToken) =>
        orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Complete(),
            cancellationToken: cancellationToken
        );
}
