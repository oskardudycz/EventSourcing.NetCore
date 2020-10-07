using System;
using System.Collections.Generic;
using Core.Aggregates;
using Orders.Orders.Enums;
using Orders.Orders.Events;
using Orders.Products;
using Orders.Products.ValueObjects;

namespace Orders.Orders
{
    public class Order: Aggregate
    {
        public Guid? ClientId { get; private set; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; private set; }

        public decimal TotalPrice { get; private set; }

        public OrderStatus Status { get; private set; }

        public Guid? PaymentId { get; private set; }

        public static Order Initialize(
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice)
        {
            var orderId = Guid.NewGuid();

            return new Order(
                orderId,
                clientId,
                productItems,
                totalPrice
            );
        }

        private Order(Guid id, Guid clientId, IReadOnlyList<PricedProductItem> productItems, decimal totalPrice)
        {
            var @event = OrderInitialized.Create(
                id,
                clientId,
                productItems,
                totalPrice,
                DateTime.UtcNow
            );

            Enqueue(@event);
            Apply(@event);
        }

        private void Apply(OrderInitialized @event)
        {
            Version++;

            Id = @event.OrderId;
            ClientId = @event.ClientId;
            ProductItems = @event.ProductItems;
            Status = OrderStatus.Initialized;
        }

        public void RecordPayment(Guid paymentId, DateTime recordedAt)
        {
            var @event = OrderPaymentRecorded.Create(
                Id,
                paymentId,
                ProductItems,
                TotalPrice,
                recordedAt
            );

            Enqueue(@event);
            Apply(@event);
        }

        private void Apply(OrderPaymentRecorded @event)
        {
            Version++;

            PaymentId = @event.PaymentId;
            Status = OrderStatus.Paid;
        }

        public void Complete()
        {
            if(Status != OrderStatus.Paid)
                throw new InvalidOperationException($"Cannot complete a not paid order.");

            var @event = OrderCompleted.Create(Id, DateTime.UtcNow);

            Enqueue(@event);
            Apply(@event);
        }

        private void Apply(OrderCompleted @event)
        {
            Version++;

            Status = OrderStatus.Completed;
        }

        public void Cancel(OrderCancellationReason cancellationReason)
        {
            if(OrderStatus.Closed.HasFlag(Status))
                throw new InvalidOperationException($"Cannot cancel a closed order.");

            var @event = OrderCancelled.Create(Id, PaymentId, DateTime.UtcNow);

            Enqueue(@event);
            Apply(@event);
        }

        private void Apply(OrderCancelled @event)
        {
            Version++;

            Status = OrderStatus.Cancelled;
        }
    }
}
