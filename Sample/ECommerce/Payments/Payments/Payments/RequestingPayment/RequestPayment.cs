using Core.Commands;
using Core.Marten.Repository;

namespace Payments.Payments.RequestingPayment;

public record RequestPayment(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
)
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

    public HandleRequestPayment(IMartenRepository<Payment> paymentRepository) =>
        this.paymentRepository = paymentRepository;

    public Task Handle(RequestPayment command, CancellationToken ct)
    {
        var (paymentId, orderId, amount) = command;

        return paymentRepository.Add(
            Payment.Initialize(paymentId, orderId, amount),
            ct
        );
    }
}
