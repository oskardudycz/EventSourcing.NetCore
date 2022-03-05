using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Orders.Orders.CancellingOrder;

public record CancelOrder(
    Guid OrderId,
    OrderCancellationReason CancellationReason
): ICommand
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
    private readonly IMartenAppendScope scope;

    public HandleCancelOrder(
        IMartenRepository<Order> orderRepository,
        IMartenAppendScope scope
    )
    {
        this.orderRepository = orderRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CancelOrder command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, traceMetadata) =>
            orderRepository.GetAndUpdate(
                command.OrderId,
                order => order.Cancel(command.CancellationReason),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
