using System;
using Core.Commands;

namespace Orders.Orders.Commands
{
    public class RecordOrderPayment: ICommand
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public DateTime PaymentRecordedAt { get; }

        public RecordOrderPayment(Guid orderId, Guid paymentId, DateTime paymentRecordedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            PaymentRecordedAt = paymentRecordedAt;
        }
    }
}
