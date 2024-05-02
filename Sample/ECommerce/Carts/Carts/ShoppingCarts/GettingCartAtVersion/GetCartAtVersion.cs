using Carts.ShoppingCarts.GettingCartById;
using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Carts.ShoppingCarts.GettingCartAtVersion;

public record GetCartAtVersion(
    Guid CartId,
    long Version
)
{
    public static GetCartAtVersion Create(Guid? cartId, long? version)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (version is null or < 0)
            throw new ArgumentOutOfRangeException(nameof(version));

        return new GetCartAtVersion(cartId.Value, version.Value);
    }
}

internal class HandleGetCartAtVersion(IQuerySession querySession):
    IQueryHandler<GetCartAtVersion, ShoppingCartDetails>
{
    public async Task<ShoppingCartDetails> Handle(GetCartAtVersion query, CancellationToken cancellationToken)
    {
        var (cartId, version) = query;
        return await querySession.Events.AggregateStreamAsync<ShoppingCartDetails>(cartId, version, token: cancellationToken)
               ?? throw AggregateNotFoundException.For<ShoppingCart>(cartId);
    }
}
