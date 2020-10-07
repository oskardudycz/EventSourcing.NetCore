using System;
using Core.Commands;
using Payments.Payments.Enums;

namespace Payments.Payments.Commands
{
    public class DiscardPayment: ICommand
    {
        public Guid PaymentId { get; }

        public DiscardReason DiscardReason { get; }

        public DiscardPayment(Guid paymentId, DiscardReason discardReason)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
        }
    }
}
