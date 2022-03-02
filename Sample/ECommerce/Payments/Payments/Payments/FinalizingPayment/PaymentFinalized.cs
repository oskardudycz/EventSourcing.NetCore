using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Marten;
using Payments.Payments.CompletingPayment;

namespace Payments.Payments.FinalizingPayment;

public record PaymentFinalized(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    DateTime FinalizedAt
): IExternalEvent
{
    public static PaymentFinalized Create(Guid paymentId, Guid orderId, decimal amount, DateTime finalizedAt)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (finalizedAt == default)
            throw new ArgumentOutOfRangeException(nameof(finalizedAt));

        return new PaymentFinalized(paymentId, orderId, amount, finalizedAt);
    }
}

public class TransformIntoPaymentFinalized:
    IEventHandler<PaymentCompleted>
{
    private readonly IEventBus eventBus;
    private readonly IQuerySession querySession;

    public TransformIntoPaymentFinalized(
        IEventBus eventBus,
        IQuerySession querySession
    )
    {
        this.eventBus = eventBus;
        this.querySession = querySession;
    }

    public async Task Handle(PaymentCompleted @event, CancellationToken cancellationToken)
    {
        var (paymentId, completedAt) = @event;

        var payment = await querySession.LoadAsync<Payment>(paymentId, cancellationToken);

        var externalEvent = PaymentFinalized.Create(
            paymentId,
            payment!.OrderId,
            payment.Amount,
            completedAt
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }
}
