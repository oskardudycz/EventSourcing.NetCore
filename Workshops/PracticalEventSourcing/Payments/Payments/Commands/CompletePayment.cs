using System;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class CompletePayment: ICommand
    {
        public Guid PaymentId { get; }

        public CompletePayment(
            Guid paymentId,
            DateTime completedAt)
        {
            PaymentId = paymentId;
        }
    }
}
