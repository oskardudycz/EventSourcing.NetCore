using System;
using Ardalis.GuardClauses;
using Core.Commands;
using Orders.Orders.Enums;

namespace Orders.Orders.Commands
{
    public class CancelOrder: ICommand
    {
        public Guid OrderId { get; }

        public OrderCancellationReason CancellationReason { get; }

        private CancelOrder(Guid orderId, OrderCancellationReason cancellationReason)
        {
            OrderId = orderId;
            CancellationReason = cancellationReason;
        }

        public static CancelOrder Create(Guid? orderId, OrderCancellationReason? cancellationReason)
        {
            if (!orderId.HasValue)
                throw new ArgumentNullException(nameof(orderId));

            if (!cancellationReason.HasValue)
                throw new ArgumentNullException(nameof(cancellationReason));

            return new CancelOrder(orderId.Value, cancellationReason.Value);
        }
    }
}
