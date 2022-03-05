using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Payments.Payments.RequestingPayment;

public record RequestPayment(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
): ICommand
{
    public static RequestPayment Create(
        Guid? paymentId,
        Guid? orderId,
        decimal? amount
    )
    {
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (orderId == null || orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (amount is null or <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        return new RequestPayment(paymentId.Value, orderId.Value, amount.Value);
    }
}

public class HandleRequestPayment:
    ICommandHandler<RequestPayment>
{
    private readonly IMartenRepository<Payment> paymentRepository;
    private readonly IMartenAppendScope scope;

    public HandleRequestPayment(
        IMartenRepository<Payment> paymentRepository,
        IMartenAppendScope scope
    )
    {
        this.paymentRepository = paymentRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(RequestPayment command, CancellationToken cancellationToken)
    {
        var (paymentId, orderId, amount) = command;

        await scope.Do((_, eventMetadata) =>
            paymentRepository.Add(
                Payment.Initialize(paymentId, orderId, amount),
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
