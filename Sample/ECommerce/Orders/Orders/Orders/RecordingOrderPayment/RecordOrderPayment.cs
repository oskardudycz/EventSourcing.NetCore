using System;
using System.Threading;
using System.Threading.Tasks;
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
        public static RecordOrderPayment Create(Guid? orderId, Guid? paymentId, DateTime? paymentRecordedAt)
        {
            if (orderId == null || orderId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(orderId));
            if (paymentId == null || paymentId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(paymentId));
            if (paymentRecordedAt == null || paymentRecordedAt == default(DateTime))
                throw new ArgumentOutOfRangeException(nameof(paymentRecordedAt));

            return new RecordOrderPayment(orderId.Value, paymentId.Value, paymentRecordedAt.Value);
        }
    }
    public class HandleRecordOrderPayment :
        ICommandHandler<RecordOrderPayment>
    {
        private readonly IRepository<Order> orderRepository;

        public HandleRecordOrderPayment(IRepository<Order> orderRepository)
        {
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
