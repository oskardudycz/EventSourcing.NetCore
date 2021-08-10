using System;
using Core.Events;

namespace Payments.Payments.RequestingPayment
{
    public class PaymentRequested: IEvent
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public decimal Amount { get; }

        private PaymentRequested(Guid paymentId, Guid orderId, decimal amount)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
        }

        public static PaymentRequested Create(Guid paymentId, Guid orderId, in decimal amount)
        {
            if (paymentId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(paymentId));
            if (orderId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(orderId));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            return new PaymentRequested(paymentId, orderId, amount);
        }
    }
}
