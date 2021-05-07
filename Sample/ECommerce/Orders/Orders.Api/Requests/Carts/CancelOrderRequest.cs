using Orders.Orders.Enums;

namespace Orders.Api.Requests.Carts
{
    public record CancelOrderRequest(
        OrderCancellationReason? CancellationReason
    );
}
