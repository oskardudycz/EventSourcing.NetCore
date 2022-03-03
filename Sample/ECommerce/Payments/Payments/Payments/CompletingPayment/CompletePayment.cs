using Core.Commands;
using Core.Marten.OptimisticConcurrency;
using Core.Marten.Repository;
using MediatR;
using Payments.Payments.DiscardingPayment;

namespace Payments.Payments.CompletingPayment;

public record CompletePayment(
    Guid PaymentId
): ICommand
{
    public static CompletePayment Create(Guid? paymentId)
    {
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));

        return new CompletePayment(paymentId.Value);
    }
}

public class HandleCompletePayment:
    ICommandHandler<CompletePayment>
{
    private readonly IMartenRepository<Payment> paymentRepository;
    private readonly MartenOptimisticConcurrencyScope scope;

    public HandleCompletePayment(
        IMartenRepository<Payment> paymentRepository,
        MartenOptimisticConcurrencyScope scope
    )
    {
        this.paymentRepository = paymentRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CompletePayment command, CancellationToken cancellationToken)
    {
        var paymentId = command.PaymentId;

        await scope.Do(async expectedVersion =>
            {
                try
                {
                    return await paymentRepository.GetAndUpdate(
                        paymentId,
                        payment => payment.Complete(),
                        expectedVersion,
                        cancellationToken);
                }
                catch
                {
                    return await paymentRepository.GetAndUpdate(
                        paymentId,
                        payment => payment.Discard(DiscardReason.UnexpectedError),
                        expectedVersion,
                        cancellationToken);
                }
            }
        );
        return Unit.Value;
    }
}
