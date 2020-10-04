using System;
using Core.Events;

namespace Payments.Payments.Events.External
{
    public class PaymentFailed: IExternalEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public PaymentFailed(Guid orderId, Guid paymentId, decimal amount)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
        }
    }
}
