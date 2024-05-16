using Core.Commands;
using Core.Marten.Repository;

namespace Orders.Orders.RecordingOrderPayment;

public record RecordOrderPayment(
    Guid OrderId,
    Guid PaymentId,
    DateTimeOffset PaymentRecordedAt
)
{
    public static RecordOrderPayment Create(Guid? orderId, Guid? paymentId, DateTimeOffset? paymentRecordedAt)
    {
        if (orderId == null || orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (paymentRecordedAt == null || paymentRecordedAt == default(DateTimeOffset))
            throw new ArgumentOutOfRangeException(nameof(paymentRecordedAt));

        return new RecordOrderPayment(orderId.Value, paymentId.Value, paymentRecordedAt.Value);
    }
}

public class HandleRecordOrderPayment(IMartenRepository<Order> orderRepository):
    ICommandHandler<RecordOrderPayment>
{
    public Task Handle(RecordOrderPayment command, CancellationToken ct)
    {
        var (orderId, paymentId, recordedAt) = command;

        return orderRepository.GetAndUpdate(
            orderId,
            order => order.RecordPayment(paymentId, recordedAt),
            ct: ct
        );
    }
}
