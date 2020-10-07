using System;
using Orders.Orders.Enums;

namespace Orders.Api.Requests.Carts
{
    public class CancelOrderRequest
    {
        public OrderCancellationReason CancellationReason { get; }

        public CancelOrderRequest(OrderCancellationReason cancellationReason)
        {
            CancellationReason = cancellationReason;
        }
    }
}
