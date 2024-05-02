using Carts.ShoppingCarts.GettingCartById;
using Core.EventStoreDB.Events;
using Core.Exceptions;
using Core.Queries;
using EventStore.Client;

namespace Carts.ShoppingCarts.GettingCartAtVersion;

public record GetCartAtVersion(
    Guid CartId,
    ulong Version
)
{
    public static GetCartAtVersion Create(Guid? cartId, ulong? version)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (version == null)
            throw new ArgumentOutOfRangeException(nameof(version));

        return new GetCartAtVersion(cartId.Value, version.Value);
    }
}

internal class HandleGetCartAtVersion(EventStoreClient eventStore):
    IQueryHandler<GetCartAtVersion, ShoppingCartDetails>
{
    public async Task<ShoppingCartDetails> Handle(GetCartAtVersion request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.AggregateStream<ShoppingCartDetails>(
            request.CartId,
            cancellationToken,
            request.Version
        );

        if (cart == null)
            throw AggregateNotFoundException.For<ShoppingCart>(request.CartId);

        return cart;
    }
}
