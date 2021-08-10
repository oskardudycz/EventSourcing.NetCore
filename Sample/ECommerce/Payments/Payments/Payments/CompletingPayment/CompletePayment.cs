using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;
using Payments.Payments.DiscardingPayment;

namespace Payments.Payments.CompletingPayment
{
    public class CompletePayment: ICommand
    {
        public Guid PaymentId { get; }

        private CompletePayment(
            Guid paymentId)
        {
            PaymentId = paymentId;
        }

        public static CompletePayment Create(Guid? paymentId)
        {
            if (paymentId == null || paymentId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(paymentId));

            return new CompletePayment(paymentId.Value);
        }
    }

    public class HandleCompletePayment:
        ICommandHandler<CompletePayment>
    {
        private readonly IRepository<Payment> paymentRepository;

        public HandleCompletePayment(
            IRepository<Payment> paymentRepository)
        {
            this.paymentRepository = paymentRepository;
        }

        public async Task<Unit> Handle(CompletePayment command, CancellationToken cancellationToken)
        {
            try
            {
                await paymentRepository.GetAndUpdate(
                    command.PaymentId,
                    payment => payment.Complete(),
                    cancellationToken);
            }
            catch
            {
                await paymentRepository.GetAndUpdate(
                    command.PaymentId,
                    payment => payment.Discard(DiscardReason.UnexpectedError),
                    cancellationToken);
            }
            return Unit.Value;
        }
    }
}
