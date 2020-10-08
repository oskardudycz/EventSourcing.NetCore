using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class CompletePayment: ICommand
    {
        public Guid PaymentId { get; }

        private CompletePayment(
            Guid paymentId)
        {
            PaymentId = paymentId;
        }

        public static CompletePayment Create(Guid paymentId)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));

            return new CompletePayment(paymentId);
        }
    }
}
