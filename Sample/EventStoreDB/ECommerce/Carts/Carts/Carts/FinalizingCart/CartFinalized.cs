using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.Products;
using Core.Events;
using Core.Exceptions;
using Marten;

namespace Carts.Carts.FinalizingCart
{
    public class CartFinalized: IExternalEvent
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; }

        public decimal TotalPrice { get; }

        public DateTime FinalizedAt { get; }

        private CartFinalized(
            Guid cartId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice,
            DateTime finalizedAt)
        {
            CartId = cartId;
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
            FinalizedAt = finalizedAt;
        }

        public static CartFinalized Create(
            Guid cartId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice,
            DateTime finalizedAt)
        {
            return new(cartId, clientId, productItems, totalPrice, finalizedAt);
        }
    }

    internal class HandleCartFinalized : IEventHandler<CartConfirmed>
    {
        private readonly IQuerySession querySession;
        private readonly IEventBus eventBus;

        public HandleCartFinalized(
            IQuerySession querySession,
            IEventBus eventBus
        )
        {
            this.querySession = querySession;
            this.eventBus = eventBus;
        }

        public async Task Handle(CartConfirmed @event, CancellationToken cancellationToken)
        {
            var cart = await querySession.LoadAsync<Cart>(@event.CartId, cancellationToken)
                       ?? throw  AggregateNotFoundException.For<Cart>(@event.CartId);

            var externalEvent = CartFinalized.Create(
                @event.CartId,
                cart.ClientId,
                cart.ProductItems.ToList(),
                cart.TotalPrice,
                @event.ConfirmedAt
            );

            await eventBus.Publish(externalEvent);
        }
    }
}
