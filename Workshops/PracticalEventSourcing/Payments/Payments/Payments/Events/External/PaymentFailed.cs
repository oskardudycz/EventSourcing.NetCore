using System;
using Core.Events;
using Payments.Payments.Enums;

namespace Payments.Payments.Events.External
{
    public class PaymentFailed: IExternalEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public DateTime FailedAt { get; }

        public PaymentFailReason FailReason { get; }

        private PaymentFailed(
            Guid paymentId,
            Guid orderId,
            decimal amount,
            DateTime failedAt,
            PaymentFailReason failReason
        )
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
            FailedAt = failedAt;
            FailReason = failReason;
        }


        public static PaymentFailed Create(
            Guid paymentId,
            Guid orderId,
            decimal amount,
            DateTime failedAt,
            PaymentFailReason failReason
        )
        {
            return new PaymentFailed(paymentId, orderId, amount, failedAt, failReason);
        }
    }
}
