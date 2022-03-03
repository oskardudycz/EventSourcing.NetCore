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
    IEventHandler<PaymentDiscarded>,
    IEventHandler<PaymentTimedOut>
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

    public async Task Handle(PaymentDiscarded @event, CancellationToken cancellationToken)
    {
        var payment = await querySession.LoadAsync<Payment>(@event.PaymentId, cancellationToken);

        var externalEvent = PaymentFailed.Create(
            @event.PaymentId,
            payment!.OrderId,
            payment.Amount,
            @event.DiscardedAt,
            PaymentFailReason.Discarded
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }

    public async Task Handle(PaymentTimedOut @event, CancellationToken cancellationToken)
    {
        var payment = await querySession.LoadAsync<Payment>(@event.PaymentId, cancellationToken);

        var externalEvent = PaymentFailed.Create(
            @event.PaymentId,
            payment!.OrderId,
            payment.Amount,
            @event.TimedOutAt,
            PaymentFailReason.Discarded
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }
}
