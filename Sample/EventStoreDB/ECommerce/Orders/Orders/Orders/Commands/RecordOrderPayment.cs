using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Orders.Orders.Commands
{
    public class RecordOrderPayment: ICommand
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public DateTime PaymentRecordedAt { get; }

        private RecordOrderPayment(Guid orderId, Guid paymentId, DateTime paymentRecordedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            PaymentRecordedAt = paymentRecordedAt;
        }
        public static RecordOrderPayment Create(Guid orderId, Guid paymentId, DateTime paymentRecordedAt)
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(paymentRecordedAt, nameof(paymentRecordedAt));

            return new RecordOrderPayment(orderId, paymentId, paymentRecordedAt);
        }
    }
}
