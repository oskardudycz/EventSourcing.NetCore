using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;

namespace Carts.ShoppingCarts.GettingCartById;

public class GetCartById : IQuery<ShoppingCartDetails>
{
    public Guid CartId { get; }

    private GetCartById(Guid cartId)
    {
        CartId = cartId;
    }

    public static GetCartById Create(Guid cartId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new GetCartById(cartId);
    }
}

internal class HandleGetCartById :
    IQueryHandler<GetCartById, ShoppingCartDetails?>
{
    private readonly IDocumentSession querySession;

    public HandleGetCartById(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public Task<ShoppingCartDetails?> Handle(GetCartById request, CancellationToken cancellationToken)
    {
        return querySession.LoadAsync<ShoppingCartDetails>(request.CartId, cancellationToken);
    }
}