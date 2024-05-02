using Core.Commands;
using Core.Marten.Repository;
using Payments.Payments.DiscardingPayment;

namespace Payments.Payments.CompletingPayment;

public record CompletePayment(
    Guid PaymentId
)
{
    public static CompletePayment Create(Guid? paymentId)
    {
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));

        return new CompletePayment(paymentId.Value);
    }
}

public class HandleCompletePayment(IMartenRepository<Payment> paymentRepository):
    ICommandHandler<CompletePayment>
{
    public async Task Handle(CompletePayment command, CancellationToken ct)
    {
        var paymentId = command.PaymentId;

        try
        {
            await paymentRepository.GetAndUpdate(
                paymentId,
                payment => payment.Complete(),
                ct: ct
            );
        }
        catch
        {
            await paymentRepository.GetAndUpdate(
                paymentId,
                payment => payment.Discard(DiscardReason.UnexpectedError),
                ct: ct
            );
        }
    }
}
