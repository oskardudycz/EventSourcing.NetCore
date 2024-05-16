using Core.Commands;
using Core.Marten.Repository;
using Core.Validation;

namespace Orders.Orders.CompletingOrder;

public record CompleteOrder(Guid OrderId)
{
    public static CompleteOrder For(Guid? orderId) => new(orderId.NotEmpty());
}

public class HandleCompleteOrder(IMartenRepository<Order> orderRepository, TimeProvider timeProvider):
    ICommandHandler<CompleteOrder>
{
    public Task Handle(CompleteOrder command, CancellationToken ct) =>
        orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Complete(timeProvider.GetUtcNow()),
            ct: ct
        );
}
