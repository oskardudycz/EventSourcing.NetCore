using Core.Aggregates;
using Payments.Payments.CompletingPayment;
using Payments.Payments.DiscardingPayment;
using Payments.Payments.RequestingPayment;
using Payments.Payments.TimingOutPayment;

namespace Payments.Payments;

public class Payment: Aggregate
{
    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public static Payment Initialize(Guid paymentId, Guid orderId, decimal amount) =>
        new(paymentId, orderId, amount);

    public Payment(){}

    private Payment(Guid id, Guid orderId, decimal amount)
    {
        var @event = PaymentRequested.Create(id, orderId, amount);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(PaymentRequested @event)
    {
        Id = @event.PaymentId;
        OrderId = @event.OrderId;
        Amount = @event.Amount;
    }

    public void Complete()
    {
        if(Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Completing payment in '{Status}' status is not allowed.");

        var @event = PaymentCompleted.Create(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(PaymentCompleted @event)
    {
        Status = PaymentStatus.Completed;
    }

    public void Discard(DiscardReason discardReason)
    {
        if(Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Discarding payment in '{Status}' status is not allowed.");

        var @event = PaymentDiscarded.Create(Id, discardReason, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(PaymentDiscarded @event)
    {
        Status = PaymentStatus.Failed;
    }

    public void TimeOut()
    {
        if(Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Discarding payment in '{Status}' status is not allowed.");

        var @event = PaymentTimedOut.Create(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(PaymentTimedOut @event)
    {
        Status = PaymentStatus.Failed;
    }
}
