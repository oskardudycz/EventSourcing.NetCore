using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Orders.Orders.Commands
{
    public class CompleteOrder: ICommand
    {
        public Guid OrderId { get; }

        private CompleteOrder(Guid orderId)
        {
            OrderId = orderId;
        }

        public static CompleteOrder Create(Guid orderId)
        {
            Guard.Against.Default(orderId, nameof(orderId));

            return new CompleteOrder(orderId);
        }
    }
}
