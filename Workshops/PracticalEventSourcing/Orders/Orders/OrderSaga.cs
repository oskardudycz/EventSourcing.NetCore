using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts.Events.External;
using Core.Commands;
using Core.Events;
using Orders.Orders.Commands;
using Orders.Orders.Enums;
using Orders.Orders.Events;
using Payments.Payments.Commands;
using Payments.Payments.Events.Enums;
using Payments.Payments.Events.External;
using Shipments.Packages.Commands;
using Shipments.Packages.Events.External;

namespace Orders.Orders
{
    public class OrderSaga:
        IEventHandler<CartFinalized>,
        IEventHandler<OrderInitialized>,
        IEventHandler<PaymentFinalized>,
        IEventHandler<PackageWasSent>,
        IEventHandler<ProductWasOutOfStock>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderPaymentRecorded>
    {
        // Happy path
        public Task Handle(CartFinalized @event, CancellationToken cancellationToken)
        {
            return SendCommand(new InitOrder(@event.ClientId, @event.ProductItems, @event.TotalPrice));
        }

        public Task Handle(OrderInitialized @event, CancellationToken cancellationToken)
        {
            return SendCommand(new RequestPayment(@event.OrderId, @event.TotalPrice));
        }

        public async Task Handle(PaymentFinalized @event, CancellationToken cancellationToken)
        {
            await SendCommand(new RecordOrderPayment(@event.OrderId, @event.PaymentId, @event.FinalizedAt));
        }

        public Task Handle(OrderPaymentRecorded @event, CancellationToken cancellationToken)
        {
            return SendCommand(new SentPackage(@event.OrderId, @event.ProductItems.Select(pi => pi.ProductItem).ToList()));
        }

        public Task Handle(PackageWasSent @event, CancellationToken cancellationToken)
        {
            return SendCommand(new CompleteOrder(@event.OrderId));
        }

        // Compensation
        public Task Handle(ProductWasOutOfStock @event, CancellationToken cancellationToken)
        {
            return SendCommand(new CancelOrder(@event.OrderId, OrderCancellationReason.ProductWasOutOfStock));
        }

        public Task Handle(OrderCancelled @event, CancellationToken cancellationToken)
        {
            return SendCommand(new DiscardPayment(@event.PaymentId, DiscardReason.OrderCancelled));
        }

        private static Task SendCommand(ICommand command)
        {
            return Task.FromException(new NotImplementedException());
        }
    }
}
