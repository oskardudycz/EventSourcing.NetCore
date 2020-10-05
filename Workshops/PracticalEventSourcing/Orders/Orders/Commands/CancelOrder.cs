using System;
using Core.Commands;
using Orders.Orders.Enums;

namespace Orders.Orders.Commands
{
    public class CancelOrder: ICommand
    {
        public Guid OrderId { get; }

        public OrderCancellationReason CancellationReason { get; }

        public CancelOrder(Guid orderId, OrderCancellationReason cancellationReason)
        {
            OrderId = orderId;
            CancellationReason = cancellationReason;
        }
    }
}
