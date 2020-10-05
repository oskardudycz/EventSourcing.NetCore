using System;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class RequestPayment: ICommand
    {
        public Guid OrderId { get; }

        public decimal Amount { get; }

        public RequestPayment(Guid orderId, decimal amount)
        {
            OrderId = orderId;
            Amount = amount;
        }
    }
}
