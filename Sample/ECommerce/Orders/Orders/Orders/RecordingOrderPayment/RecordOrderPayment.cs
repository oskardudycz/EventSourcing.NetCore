using Core.Commands;
using Core.Marten.OptimisticConcurrency;
using Core.Marten.Repository;
using MediatR;

namespace Orders.Orders.RecordingOrderPayment;

public record RecordOrderPayment(
    Guid OrderId,
    Guid PaymentId,
    DateTime PaymentRecordedAt
): ICommand
{
    public static RecordOrderPayment Create(Guid? orderId, Guid? paymentId, DateTime? paymentRecordedAt)
    {
        if (orderId == null || orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (paymentRecordedAt == null || paymentRecordedAt == default(DateTime))
            throw new ArgumentOutOfRangeException(nameof(paymentRecordedAt));

        return new RecordOrderPayment(orderId.Value, paymentId.Value, paymentRecordedAt.Value);
    }
}

public class HandleRecordOrderPayment:
    ICommandHandler<RecordOrderPayment>
{
    private readonly IMartenRepository<Order> orderRepository;
    private readonly MartenOptimisticConcurrencyScope scope;

    public HandleRecordOrderPayment(
        IMartenRepository<Order> orderRepository,
        MartenOptimisticConcurrencyScope scope
    )
    {
        this.orderRepository = orderRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(RecordOrderPayment command, CancellationToken cancellationToken)
    {
        var (orderId, paymentId, recordedAt) = command;
        await scope.Do(expectedVersion =>
            orderRepository.GetAndUpdate(
                orderId,
                order => order.RecordPayment(paymentId, recordedAt),
                expectedVersion,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
