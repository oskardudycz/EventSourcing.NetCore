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

        public static CancelOrder Create(Guid orderId, OrderCancellationReason cancellationReason)
        {
            Guard.Against.Default(orderId, nameof(orderId));

            return new CancelOrder(orderId, cancellationReason);
        }
    }
}
