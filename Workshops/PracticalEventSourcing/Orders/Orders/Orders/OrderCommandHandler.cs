using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;
using Orders.Orders.Commands;

namespace Orders.Orders
{
    public class OrderCommandHandler :
        ICommandHandler<InitOrder>,
        ICommandHandler<RecordOrderPayment>,
        ICommandHandler<CompleteOrder>,
        ICommandHandler<CancelOrder>
    {
        private readonly IRepository<Order> orderRepository;

        public OrderCommandHandler(IRepository<Order> orderRepository)
        {
            Guard.Against.Null(orderRepository, nameof(orderRepository));

            this.orderRepository = orderRepository;
        }

        public async Task<Unit> Handle(InitOrder command, CancellationToken cancellationToken)
        {
            var order = Order.Initialize(command.ClientId, command.ProductItems, command.TotalPrice);

            await orderRepository.Add(order, cancellationToken);

            return Unit.Value;
        }

        public Task<Unit> Handle(RecordOrderPayment command, CancellationToken cancellationToken)
        {
            return orderRepository.GetAndUpdate(
                command.OrderId,
                order => order.RecordPayment(command.PaymentId, command.PaymentRecordedAt),
                cancellationToken);
        }

        public Task<Unit> Handle(CompleteOrder command, CancellationToken cancellationToken)
        {
            return orderRepository.GetAndUpdate(
                command.OrderId,
                order => order.Complete(),
                cancellationToken);
        }

        public Task<Unit> Handle(CancelOrder command, CancellationToken cancellationToken)
        {
            return orderRepository.GetAndUpdate(
                command.OrderId,
                order => order.Cancel(command.CancellationReason),
                cancellationToken);
        }
    }
}
