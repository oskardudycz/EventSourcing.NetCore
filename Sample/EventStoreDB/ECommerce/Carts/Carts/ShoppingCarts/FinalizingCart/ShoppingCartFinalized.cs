using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.Products;
using Core.Events;
using Core.Exceptions;
using Marten;

namespace Carts.ShoppingCarts.FinalizingCart;

public record ShoppingCartFinalized(
    Guid CartId,
    Guid ClientId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal TotalPrice,
    DateTime FinalizedAt
): IExternalEvent
{
    public static ShoppingCartFinalized Create(
        Guid cartId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTime finalizedAt)
    {
        return new ShoppingCartFinalized(cartId, clientId, productItems, totalPrice, finalizedAt);
    }
}

internal class HandleCartFinalized: IEventHandler<EventEnvelope<ShoppingCartConfirmed>>
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

    public async Task Handle(EventEnvelope<ShoppingCartConfirmed> @event, CancellationToken cancellationToken)
    {
        var cart = await querySession.LoadAsync<ShoppingCart>(@event.Data.CartId, cancellationToken)
                   ?? throw AggregateNotFoundException.For<ShoppingCart>(@event.Data.CartId);

        // TODO: This should be handled internally by event bus, or this event should be stored in the outbox stream
        var externalEvent = new EventEnvelope<ShoppingCartFinalized>(
            ShoppingCartFinalized.Create(
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
