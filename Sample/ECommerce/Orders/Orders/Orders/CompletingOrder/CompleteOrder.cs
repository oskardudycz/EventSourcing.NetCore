using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace Orders.Orders.CompletingOrder;

public class CompleteOrder: ICommand
{
    public Guid OrderId { get; }

    private CompleteOrder(Guid orderId)
    {
        OrderId = orderId;
    }

    public static CompleteOrder Create(Guid? orderId)
    {
        if (orderId == null || orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));

        return new CompleteOrder(orderId.Value);
    }
}

public class HandleCompleteOrder :
    ICommandHandler<CompleteOrder>
{
    private readonly IMartenRepository<Order> orderRepository;

    public HandleCompleteOrder(IMartenRepository<Order> orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public Task<Unit> Handle(CompleteOrder command, CancellationToken cancellationToken)
    {
        return orderRepository.GetAndUpdate(
            command.OrderId,
            order => order.Complete(),
            cancellationToken);
    }
}
