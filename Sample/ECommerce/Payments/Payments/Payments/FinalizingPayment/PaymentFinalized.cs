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

public class TransformIntoPaymentFinalized(
    IEventBus eventBus,
    IQuerySession querySession):
    IEventHandler<EventEnvelope<PaymentCompleted>>
{
    public async Task Handle(EventEnvelope<PaymentCompleted> @event, CancellationToken cancellationToken)
    {
        var (paymentId, completedAt) = @event.Data;

        var payment = await querySession.LoadAsync<Payment>(paymentId, cancellationToken);

        // TODO: This should be handled internally by event bus, or this event should be stored in the outbox stream
        var externalEvent = new EventEnvelope<PaymentFinalized>(
            PaymentFinalized.Create(
                paymentId,
                payment!.OrderId,
                payment.Amount,
                completedAt
            ),
            @event.Metadata
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }
}
