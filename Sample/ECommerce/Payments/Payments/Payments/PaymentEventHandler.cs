using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Events;
using Marten;
using Payments.Payments.Enums;
using Payments.Payments.Events;
using Payments.Payments.Events.External;

namespace Payments.Payments
{
    public class PaymentEventHandler :
        IEventHandler<PaymentCompleted>,
        IEventHandler<PaymentDiscarded>,
        IEventHandler<PaymentTimedOut>
    {
        private readonly IEventBus eventBus;
        private readonly IQuerySession querySession;

        public PaymentEventHandler(
            IEventBus eventBus,
            IQuerySession querySession
            )
        {
            Guard.Against.Null(eventBus, nameof(eventBus));

            this.eventBus = eventBus;
            this.querySession = querySession;
        }

        public async Task Handle(PaymentCompleted @event, CancellationToken cancellationToken)
        {
            var payment = await querySession.LoadAsync<Payment>(@event.PaymentId);

            var externalEvent = PaymentFinalized.Create(
                @event.PaymentId,
                payment!.OrderId,
                payment.Amount,
                @event.CompletedAt
            );

            await eventBus.Publish(externalEvent);
        }

        public async Task Handle(PaymentDiscarded @event, CancellationToken cancellationToken)
        {
            var payment = await querySession.LoadAsync<Payment>(@event.PaymentId);

            var externalEvent = PaymentFailed.Create(
                @event.PaymentId,
                payment!.OrderId,
                payment.Amount,
                @event.DiscardedAt,
                PaymentFailReason.Discarded
            );

            await eventBus.Publish(externalEvent);
        }

        public async Task Handle(PaymentTimedOut @event, CancellationToken cancellationToken)
        {
            var payment = await querySession.LoadAsync<Payment>(@event.PaymentId);

            var externalEvent = PaymentFailed.Create(
                @event.PaymentId,
                payment!.OrderId,
                payment.Amount,
                @event.TimedOutAt,
                PaymentFailReason.Discarded
            );

            await eventBus.Publish(externalEvent);
        }
    }
}
