using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Events;
using Marten;
using Payments.Payments.CompletingPayment;

namespace Payments.Payments.FinalizingPayment
{
    public class PaymentFinalized: IExternalEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public DateTime FinalizedAt { get; }

        private PaymentFinalized(Guid paymentId, Guid orderId, decimal amount, DateTime finalizedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
            FinalizedAt = finalizedAt;
        }
        public static PaymentFinalized Create(Guid paymentId, Guid orderId, decimal amount, DateTime finalizedAt)
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.NegativeOrZero(amount, nameof(amount));
            Guard.Against.Default(finalizedAt, nameof(finalizedAt));

            return new PaymentFinalized(paymentId, orderId, amount, finalizedAt);
        }
    }
    public class TransformIntoPaymentFinalized :
        IEventHandler<PaymentCompleted>
    {
        private readonly IEventBus eventBus;
        private readonly IQuerySession querySession;

        public TransformIntoPaymentFinalized(
            IEventBus eventBus,
            IQuerySession querySession
            )
        {
            Guard.Against.Null(eventBus, nameof(eventBus));

            this.eventBus = eventBus;
            this.querySession = querySession;
        }

        public async Task Handle(PaymentCompleted @event, CancellationToken cancellationToken)
        {
            var payment = await querySession.LoadAsync<Payment>(@event.PaymentId, cancellationToken);

            var externalEvent = PaymentFinalized.Create(
                @event.PaymentId,
                payment!.OrderId,
                payment.Amount,
                @event.CompletedAt
            );

            await eventBus.Publish(externalEvent);
        }
    }
}
