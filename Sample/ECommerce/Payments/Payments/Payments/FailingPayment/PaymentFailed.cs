using Core.Events;
using Marten;
using Payments.Payments.DiscardingPayment;
using Payments.Payments.TimingOutPayment;

namespace Payments.Payments.FailingPayment;

public record PaymentFailed(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    DateTime FailedAt,
    PaymentFailReason FailReason
): IExternalEvent
{
    public static PaymentFailed Create(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        DateTime failedAt,
        PaymentFailReason failReason
    ) => new (paymentId, orderId, amount, failedAt, failReason);
}


public class TransformIntoPaymentFailed :
    IEventHandler<EventEnvelope<PaymentDiscarded>>,
    IEventHandler<EventEnvelope<PaymentTimedOut>>
{
    private readonly IEventBus eventBus;
    private readonly IQuerySession querySession;

    public TransformIntoPaymentFailed(
        IEventBus eventBus,
        IQuerySession querySession
    )
    {
        this.eventBus = eventBus;
        this.querySession = querySession;
    }

    public async Task Handle(EventEnvelope<PaymentDiscarded> @event, CancellationToken cancellationToken)
    {
        var payment = await querySession.LoadAsync<Payment>(@event.Data.PaymentId, cancellationToken);

        // TODO: This should be handled internally by event bus, or this event should be stored in the outbox stream
        var externalEvent = new EventEnvelope<PaymentFailed>(
            PaymentFailed.Create(
                @event.Data.PaymentId,
                payment!.OrderId,
                payment.Amount,
                @event.Data.DiscardedAt,
                PaymentFailReason.Discarded
            ),
            @event.Metadata
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }

    public async Task Handle(EventEnvelope<PaymentTimedOut> @event, CancellationToken cancellationToken)
    {
        var payment = await querySession.LoadAsync<Payment>(@event.Data.PaymentId, cancellationToken);

        // TODO: This should be handled internally by event bus, or this event should be stored in the outbox stream
        var externalEvent = new EventEnvelope<PaymentFailed>(
            PaymentFailed.Create(
                @event.Data.PaymentId,
                payment!.OrderId,
                payment.Amount,
                @event.Data.TimedOutAt,
                PaymentFailReason.Discarded
            ),
            @event.Metadata
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }
}
