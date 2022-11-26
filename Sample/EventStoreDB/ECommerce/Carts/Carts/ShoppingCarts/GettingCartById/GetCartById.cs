using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Carts.ShoppingCarts.GettingCartById;

public record GetCartById(
    Guid CartId
)
{
    public static GetCartById Create(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new GetCartById(cartId.Value);
    }
}

internal class HandleGetCartById :
    IQueryHandler<GetCartById, ShoppingCartDetails>
{
    private readonly IDocumentSession querySession;

    public HandleGetCartById(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<ShoppingCartDetails> Handle(GetCartById request, CancellationToken cancellationToken)
    {
        var cart = await querySession.LoadAsync<ShoppingCartDetails>(request.CartId, cancellationToken);

        return cart ?? throw AggregateNotFoundException.For<ShoppingCart>(request.CartId);
    }
}
