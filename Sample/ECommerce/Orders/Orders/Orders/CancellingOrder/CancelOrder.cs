using Core.Commands;
using Core.Events;
using Core.Marten.Repository;
using Marten;
using Orders.Orders.GettingPending;

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

public class HandleCancelOrder(
    IMartenRepository<Order> orderRepository,
    IQuerySession querySession,
    TimeProvider timeProvider
):
    ICommandHandler<CancelOrder>,
    IEventHandler<TimeHasPassed>
{
    public Task Handle(CancelOrder command, CancellationToken ct) =>
        orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Cancel(command.CancellationReason, timeProvider.GetUtcNow()),
            ct: ct
        );

    public async Task Handle(TimeHasPassed @event, CancellationToken ct)
    {
        var orderIds = await querySession.Query<PendingOrder>()
            .Where(o => o.TimeoutAfter <= @event.Now)
            .Select(o => o.Id)
            .ToListAsync(token: ct);

        var now = timeProvider.GetUtcNow();

        foreach (var orderId in orderIds)
        {
            await orderRepository.GetAndUpdate(
                orderId,
                order => order.Cancel(OrderCancellationReason.TimedOut, now),
                ct: ct
            );
        }
    }
}
