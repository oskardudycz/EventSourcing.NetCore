using System;
using Core.Commands;

namespace Orders.Orders.Commands
{
    public class CompleteOrder: ICommand
    {
        public Guid OrderId { get; }

        public CompleteOrder(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}
