using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Orders.Orders.CancellingOrder;

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

public class HandleCancelOrder :
    ICommandHandler<CancelOrder>
{
    private readonly IRepository<Order> orderRepository;

    public HandleCancelOrder(IRepository<Order> orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public Task<Unit> Handle(CancelOrder command, CancellationToken cancellationToken)
    {
        return orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Cancel(command.CancellationReason),
            cancellationToken);
    }
}