using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Orders.Orders.CompletingOrder;

public record CompleteOrder(
    Guid OrderId
): ICommand
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
    private readonly IMartenAppendScope scope;

    public HandleCompleteOrder(
        IMartenRepository<Order> orderRepository,
        IMartenAppendScope scope
    )
    {
        this.orderRepository = orderRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CompleteOrder command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, traceMetadata) =>
            orderRepository.GetAndUpdate(
                command.OrderId,
                order => order.Complete(),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
