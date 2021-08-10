using System;
using System.Threading;
using System.Threading.Tasks;
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
