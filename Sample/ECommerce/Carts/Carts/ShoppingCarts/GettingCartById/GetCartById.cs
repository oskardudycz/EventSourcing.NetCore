using Core.Queries;
using Marten;

namespace Carts.ShoppingCarts.GettingCartById;

public record GetCartById(
    Guid CartId
)
{
    public static GetCartById Create(Guid cartId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new GetCartById(cartId);
    }
}

internal class HandleGetCartById(IQuerySession querySession):
    IQueryHandler<GetCartById, ShoppingCartDetails?>
{
    public Task<ShoppingCartDetails?> Handle(GetCartById query, CancellationToken cancellationToken) =>
        querySession.LoadAsync<ShoppingCartDetails>(query.CartId, cancellationToken);
}
