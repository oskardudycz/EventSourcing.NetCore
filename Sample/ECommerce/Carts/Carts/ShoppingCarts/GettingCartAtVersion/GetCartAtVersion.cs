using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.ShoppingCarts.GettingCartById;
using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Carts.ShoppingCarts.GettingCartAtVersion;

public record GetCartAtVersion(
    Guid CartId,
    long Version
) : IQuery<ShoppingCartDetails>
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

internal class HandleGetCartAtVersion :
    IQueryHandler<GetCartAtVersion, ShoppingCartDetails>
{
    private readonly IDocumentSession querySession;

    public HandleGetCartAtVersion(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<ShoppingCartDetails> Handle(GetCartAtVersion query, CancellationToken cancellationToken)
    {
        var (cartId, version) = query;
        return await querySession.Events.AggregateStreamAsync<ShoppingCartDetails>(cartId, version, token: cancellationToken)
               ?? throw AggregateNotFoundException.For<ShoppingCart>(cartId);
    }
}
