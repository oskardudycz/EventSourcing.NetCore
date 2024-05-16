using Core.Queries;
using Core.Validation;
using Marten;

namespace Orders.Orders.GettingOrderStatus;

public record GetOrderStatus(Guid OrderId)
{
    public static GetOrderStatus For(Guid orderId) => new(orderId.NotEmpty());
}

internal class HandleGetOrderStatus(IQuerySession querySession): IQueryHandler<GetOrderStatus, OrderDetails?>
{
    public Task<OrderDetails?> Handle(GetOrderStatus query, CancellationToken cancellationToken) =>
        querySession.LoadAsync<OrderDetails>(query.OrderId, cancellationToken);
}
