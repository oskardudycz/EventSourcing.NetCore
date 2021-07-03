using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Orders.Orders.RecordingOrderPayment
{
    public class RecordOrderPayment: ICommand
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public DateTime PaymentRecordedAt { get; }

        private RecordOrderPayment(Guid orderId, Guid paymentId, DateTime paymentRecordedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            PaymentRecordedAt = paymentRecordedAt;
        }
        public static RecordOrderPayment Create(Guid orderId, Guid paymentId, DateTime paymentRecordedAt)
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(paymentRecordedAt, nameof(paymentRecordedAt));

            return new RecordOrderPayment(orderId, paymentId, paymentRecordedAt);
        }
    }
    public class HandleRecordOrderPayment :
        ICommandHandler<RecordOrderPayment>
    {
        private readonly IRepository<Order> orderRepository;

        public HandleRecordOrderPayment(IRepository<Order> orderRepository)
        {
            Guard.Against.Null(orderRepository, nameof(orderRepository));

            this.orderRepository = orderRepository;
        }

        public Task<Unit> Handle(RecordOrderPayment command, CancellationToken cancellationToken)
        {
            return orderRepository.GetAndUpdate(
                command.OrderId,
                order => order.RecordPayment(command.PaymentId, command.PaymentRecordedAt),
                cancellationToken);
        }
    }
}
