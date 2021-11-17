using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;
using Orders.Products;

namespace Orders.Orders.InitializingOrder;

public class InitializeOrder: ICommand
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public IReadOnlyList<PricedProductItem> ProductItems { get; }
    public decimal TotalPrice { get; }

    private InitializeOrder(
        Guid orderId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice)
    {
        OrderId = orderId;
        ClientId = clientId;
        ProductItems = productItems;
        TotalPrice = totalPrice;
    }

    public static InitializeOrder Create(
        Guid? orderId,
        Guid? clientId,
        IReadOnlyList<PricedProductItem>? productItems,
        decimal? totalPrice
    )
    {
        if (!orderId.HasValue)
            throw new ArgumentNullException(nameof(orderId));
        if (!clientId.HasValue)
            throw new ArgumentNullException(nameof(clientId));
        if (productItems == null)
            throw new ArgumentNullException(nameof(productItems));
        if (!totalPrice.HasValue)
            throw new ArgumentNullException(nameof(totalPrice));

        return new InitializeOrder(orderId.Value, clientId.Value, productItems, totalPrice.Value);
    }
}
public class HandleInitializeOrder :
    ICommandHandler<InitializeOrder>
{
    private readonly IRepository<Order> orderRepository;

    public HandleInitializeOrder(IRepository<Order> orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(InitializeOrder command, CancellationToken cancellationToken)
    {
        var order = Order.Initialize(command.OrderId, command.ClientId, command.ProductItems, command.TotalPrice);

        await orderRepository.Add(order, cancellationToken);

        return Unit.Value;
    }
}