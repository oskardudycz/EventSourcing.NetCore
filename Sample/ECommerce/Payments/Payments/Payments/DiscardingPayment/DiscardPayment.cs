using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Payments.Payments.DiscardingPayment;

public record DiscardPayment(
    Guid PaymentId,
    DiscardReason DiscardReason
): ICommand
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

public class HandleDiscardPayment:
    ICommandHandler<DiscardPayment>
{
    private readonly IMartenRepository<Payment> paymentRepository;
    private readonly IMartenAppendScope scope;

    public HandleDiscardPayment(
        IMartenRepository<Payment> paymentRepository,
        IMartenAppendScope scope
    )
    {
        this.paymentRepository = paymentRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(DiscardPayment command, CancellationToken cancellationToken)
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
