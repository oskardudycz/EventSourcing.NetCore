using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Payments.Payments.TimingOutPayment;

public record TimeOutPayment(
    Guid PaymentId,
    DateTime TimedOutAt
): ICommand
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
    private readonly IMartenAppendScope scope;

    public HandleTimeOutPayment(
        IMartenRepository<Payment> paymentRepository,
        IMartenAppendScope scope
    )
    {
        this.paymentRepository = paymentRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(TimeOutPayment command, CancellationToken cancellationToken)
    {
        var (paymentId, _) = command;

        await scope.Do((expectedVersion, traceMetadata) =>
            paymentRepository.GetAndUpdate(
                paymentId,
                payment => payment.TimeOut(),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
