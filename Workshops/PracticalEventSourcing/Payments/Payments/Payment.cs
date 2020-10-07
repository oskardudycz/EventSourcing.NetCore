using System;
using Ardalis.GuardClauses;
using Core.Aggregates;
using Payments.Payments.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments
{
    public class Payment: Aggregate
    {
        public Guid OrderId { get; private set; }

        public decimal Amount { get; private set; }

        public PaymentStatus Status { get; private set; }

        public static Payment Initialize(Guid orderId, decimal amount)
        {
            var paymentId = Guid.NewGuid();

            return new Payment(paymentId, orderId, amount);
        }
        private Payment(Guid id, Guid orderId, in decimal amount)
        {
            var @event = PaymentRequested.Create(id, orderId, amount);

            Enqueue(@event);
            Apply(@event);
        }

        private void Apply(PaymentRequested @event)
        {
            Version++;

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

        private void Apply(PaymentCompleted @event)
        {
            Version++;

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

        private void Apply(PaymentDiscarded @event)
        {
            Version++;

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

        private void Apply(PaymentTimedOut @event)
        {
            Version++;

            Status = PaymentStatus.Failed;
        }
    }
}
