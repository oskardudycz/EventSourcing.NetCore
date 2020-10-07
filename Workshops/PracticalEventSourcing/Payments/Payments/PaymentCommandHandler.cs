using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;
using Payments.Payments.Commands;

namespace Payments.Payments
{
    public class PaymentCommandHandler:
        ICommandHandler<RequestPayment>,
        ICommandHandler<CompletePayment>,
        ICommandHandler<DiscardPayment>,
        ICommandHandler<TimeOutPayment>
    {
        private readonly IRepository<Payment> paymentRepository;

        public PaymentCommandHandler(
            IRepository<Payment> paymentRepository)
        {
            Guard.Against.Null(paymentRepository, nameof(paymentRepository));

            this.paymentRepository = paymentRepository;
        }

        public async Task<Unit> Handle(RequestPayment command, CancellationToken cancellationToken)
        {
            var payment = Payment.Initialize(command.OrderId, command.Amount);

            await paymentRepository.Add(payment, cancellationToken);

            return Unit.Value;
        }

        public Task<Unit> Handle(CompletePayment command, CancellationToken cancellationToken)
        {
            return paymentRepository.GetAndUpdate(
                command.PaymentId,
                payment => payment.Complete(),
                cancellationToken);
        }

        public Task<Unit> Handle(DiscardPayment command, CancellationToken cancellationToken)
        {
            return paymentRepository.GetAndUpdate(
                command.PaymentId,
                payment => payment.Discard(),
                cancellationToken);
        }

        public Task<Unit> Handle(TimeOutPayment command, CancellationToken cancellationToken)
        {
            return paymentRepository.GetAndUpdate(
                command.PaymentId,
                payment => payment.TimeOUt(),
                cancellationToken);
        }
    }
}
