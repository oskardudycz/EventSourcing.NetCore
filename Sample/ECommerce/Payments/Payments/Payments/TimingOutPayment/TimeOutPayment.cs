using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace Payments.Payments.TimingOutPayment;

public class TimeOutPayment: ICommand
{
    public Guid PaymentId { get; }

    public DateTime TimedOutAt { get; }

    private TimeOutPayment(Guid paymentId, DateTime timedOutAt)
    {
        PaymentId = paymentId;
        TimedOutAt = timedOutAt;
    }

    public static TimeOutPayment Create(Guid? paymentId, DateTime? timedOutAt)
    {
        if (paymentId == null || paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (timedOutAt == null || timedOutAt == default(DateTime))
            throw new ArgumentOutOfRangeException(nameof(timedOutAt));

        return new TimeOutPayment(paymentId.Value, timedOutAt.Value);
    }
}
public class HandleTimeOutPayment:
    ICommandHandler<TimeOutPayment>
{
    private readonly IMartenRepository<Payment> paymentRepository;

    public HandleTimeOutPayment(
        IMartenRepository<Payment> paymentRepository)
    {
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
