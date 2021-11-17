using Orders.Orders.CancellingOrder;

namespace Orders.Api.Requests.Carts;

public record CancelOrderRequest(
    OrderCancellationReason? CancellationReason
);