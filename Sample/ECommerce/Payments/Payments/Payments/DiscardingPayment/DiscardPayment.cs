using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Payments.Payments.DiscardingPayment
{
    public class DiscardPayment: ICommand
    {
        public Guid PaymentId { get; }

        public DiscardReason DiscardReason { get; }

        private DiscardPayment(Guid paymentId, DiscardReason discardReason)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
        }

        public static DiscardPayment Create(Guid? paymentId, DiscardReason? discardReason)
        {
            if (paymentId == null || paymentId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(paymentId));
            if (discardReason is null or default(DiscardReason))
                throw new ArgumentOutOfRangeException(nameof(paymentId));

            return new DiscardPayment(paymentId.Value, discardReason.Value);
        }
    }

    public class HandleDiscardPayment:
        ICommandHandler<DiscardPayment>
    {
        private readonly IRepository<Payment> paymentRepository;

        public HandleDiscardPayment(
            IRepository<Payment> paymentRepository)
        {
            this.paymentRepository = paymentRepository;
        }

        public Task<Unit> Handle(DiscardPayment command, CancellationToken cancellationToken)
        {
            return paymentRepository.GetAndUpdate(
                command.PaymentId,
                payment => payment.Discard(command.DiscardReason),
                cancellationToken);
        }
    }
}
