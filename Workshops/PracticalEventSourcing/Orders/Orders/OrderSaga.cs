using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Events;
using Orders.Carts.Events;
using Orders.Orders.Commands;
using Orders.Orders.Enums;
using Orders.Orders.Events;
using Orders.Payments.Commands;
using Orders.Payments.Events;
using Orders.Products.ValueObjects;
using Orders.Shipments.Commands;
using Orders.Shipments.Events;
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
            return SendCommand(RequestPayment.Create(@event.OrderId, @event.TotalPrice));
        }

        public async Task Handle(PaymentFinalized @event, CancellationToken cancellationToken)
        {
            await SendCommand(new RecordOrderPayment(@event.OrderId, @event.PaymentId, @event.FinalizedAt));
        }

        public Task Handle(OrderPaymentRecorded @event, CancellationToken cancellationToken)
        {
            return SendCommand(
                SendPackage.Create(
                    @event.OrderId,
                    @event.ProductItems.Select(pi => new ProductItem(pi.ProductId, pi.Quantity)).ToList()
                )
            );
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
            if (!@event.PaymentId.HasValue)
                return Task.CompletedTask;

            return SendCommand(DiscardPayment.Create(@event.PaymentId.Value));
        }

        private static Task SendCommand(ICommand command)
        {
            return Task.FromException(new NotImplementedException());
        }
    }
}
