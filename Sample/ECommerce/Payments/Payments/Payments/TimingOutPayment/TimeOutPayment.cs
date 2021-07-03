using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Payments.Payments.TimingOutPayment
{
    public class TimeOutPayment: ICommand
    {
        public Guid PaymentId { get; }

        public DateTime TimedOutAt { get; }

        private TimeOutPayment(Guid paymentId, DateTime timedOutAt)
        {
            PaymentId = paymentId;
            TimedOutAt = timedOutAt;
        }

        public static TimeOutPayment Create(Guid paymentId, DateTime timedOutAt)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(timedOutAt, nameof(timedOutAt));

            return new TimeOutPayment(paymentId, timedOutAt);
        }
    }
    public class HandleTimeOutPayment:
        ICommandHandler<TimeOutPayment>
    {
        private readonly IRepository<Payment> paymentRepository;

        public HandleTimeOutPayment(
            IRepository<Payment> paymentRepository)
        {
            Guard.Against.Null(paymentRepository, nameof(paymentRepository));

            this.paymentRepository = paymentRepository;
        }

        public Task<Unit> Handle(TimeOutPayment command, CancellationToken cancellationToken)
        {
            return paymentRepository.GetAndUpdate(
                command.PaymentId,
                payment => payment.TimeOut(),
                cancellationToken);
        }
    }
}
