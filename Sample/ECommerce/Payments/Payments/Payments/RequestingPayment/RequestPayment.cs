using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Payments.Payments.RequestingPayment
{
    public class RequestPayment: ICommand
    {
        public Guid PaymentId { get; }

        public Guid OrderId { get; }

        public decimal Amount { get; }

        private RequestPayment(
            Guid paymentId,
            Guid orderId,
            decimal amount
        )
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
        }
        public static RequestPayment Create(
            Guid paymentId,
            Guid orderId,
            decimal amount
        )
        {
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.NegativeOrZero(amount, nameof(amount));

            return new RequestPayment(paymentId, orderId, amount);
        }
    }
    public class HandleRequestPayment:
        ICommandHandler<RequestPayment>
    {
        private readonly IRepository<Payment> paymentRepository;

        public HandleRequestPayment(
            IRepository<Payment> paymentRepository)
        {
            Guard.Against.Null(paymentRepository, nameof(paymentRepository));

            this.paymentRepository = paymentRepository;
        }

        public async Task<Unit> Handle(RequestPayment command, CancellationToken cancellationToken)
        {
            var payment = Payment.Initialize(command.PaymentId, command.OrderId, command.Amount);

            await paymentRepository.Add(payment, cancellationToken);

            return Unit.Value;
        }
    }
}
