using System;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class RequestPayment: ICommand
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public RequestPayment(Guid orderId, Guid paymentId, decimal amount)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
        }
    }
}
