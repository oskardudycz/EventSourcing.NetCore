using Core.Commands;
using Core.Marten.Repository;

namespace Payments.Payments.DiscardingPayment;

public record DiscardPayment(
    Guid PaymentId,
    DiscardReason DiscardReason
)
{
    public static DiscardPayment Create(Guid? paymentId, DiscardReason? discardReason)
    {
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (discardReason is null or default(DiscardReason))
            throw new ArgumentOutOfRangeException(nameof(paymentId));

        return new DiscardPayment(paymentId.Value, discardReason.Value);
    }
}

public class HandleDiscardPayment(IMartenRepository<Payment> paymentRepository):
    ICommandHandler<DiscardPayment>
{
    public Task Handle(DiscardPayment command, CancellationToken ct)
    {
        var (paymentId, _) = command;

        return paymentRepository.GetAndUpdate(
            paymentId,
            payment => payment.TimeOut(),
            ct: ct
        );
    }
}
