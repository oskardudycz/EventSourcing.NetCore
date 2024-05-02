using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.Products;
using Core.Events;
using Core.Exceptions;
using Marten;

namespace Carts.ShoppingCarts.FinalizingCart;

public record CartFinalized(
    Guid CartId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice,
    DateTime FinalizedAt
): IExternalEvent
{
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

internal class HandleCartFinalized(
    IQuerySession querySession,
    IEventBus eventBus): IEventHandler<EventEnvelope<ShoppingCartConfirmed>>
{
    public async Task Handle(EventEnvelope<ShoppingCartConfirmed> @event, CancellationToken cancellationToken)
    {
        var cart = await querySession.LoadAsync<ShoppingCart>(@event.Data.CartId, cancellationToken)
                   ?? throw AggregateNotFoundException.For<ShoppingCart>(@event.Data.CartId);

        // TODO: This should be handled internally by event bus, or this event should be stored in the outbox stream
        var externalEvent = new EventEnvelope<CartFinalized>(
            CartFinalized.Create(
                @event.Data.CartId,
                cart.ClientId,
                cart.ProductItems.ToList(),
                cart.TotalPrice,
                @event.Data.ConfirmedAt
            ),
            @event.Metadata
        );

        await eventBus.Publish(externalEvent, cancellationToken);
    }
}
