using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
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

        public static DiscardPayment Create(Guid paymentId, DiscardReason discardReason)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));

            return new DiscardPayment(paymentId, discardReason);
        }
    }

    public class HandleDiscardPayment:
        ICommandHandler<DiscardPayment>
    {
        private readonly IRepository<Payment> paymentRepository;

        public HandleDiscardPayment(
            IRepository<Payment> paymentRepository)
        {
            Guard.Against.Null(paymentRepository, nameof(paymentRepository));

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
