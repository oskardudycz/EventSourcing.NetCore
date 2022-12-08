using Core.Commands;
using Core.Marten.Repository;

namespace Payments.Payments.TimingOutPayment;

public record TimeOutPayment(
    Guid PaymentId,
    DateTime TimedOutAt
)
{
    public static TimeOutPayment Create(Guid? paymentId, DateTime? timedOutAt)
    {
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (timedOutAt == null || timedOutAt == default(DateTime))
            throw new ArgumentOutOfRangeException(nameof(timedOutAt));

        return new TimeOutPayment(paymentId.Value, timedOutAt.Value);
    }
}

public class HandleTimeOutPayment:
    ICommandHandler<TimeOutPayment>
{
    private readonly IMartenRepository<Payment> paymentRepository;

    public HandleTimeOutPayment(IMartenRepository<Payment> paymentRepository) =>
        this.paymentRepository = paymentRepository;

    public Task Handle(TimeOutPayment command, CancellationToken ct)
    {
        var (paymentId, _) = command;

        return paymentRepository.GetAndUpdate(
            paymentId,
            payment => payment.TimeOut(),
            ct: ct
        );
    }
}
