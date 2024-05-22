using Core.Exceptions;
using Core.Queries;
using Core.Validation;
using Marten;
using Polly;

namespace Carts.ShoppingCarts.GettingCartById;

public record GetCartById(
    Guid CartId,
    int? ExpectedVersion
)
{
    public static GetCartById From(Guid cartId, int? expectedVersion) =>
        new(cartId.NotEmpty(), expectedVersion);
}

internal class HandleGetCartById(IQuerySession querySession):
    IQueryHandler<GetCartById, ShoppingCartDetails>
{
    public async Task<ShoppingCartDetails> Handle(GetCartById query, CancellationToken token)
    {
        var expectedVersion = query.ExpectedVersion;

        if (!expectedVersion.HasValue)
            return await querySession.LoadAsync<ShoppingCartDetails>(query.CartId, token)
                   ?? throw AggregateNotFoundException.For<ShoppingCart>(query.CartId);

        return await Policy
                   .HandleResult<ShoppingCartDetails?>(cart =>
                       cart == null || cart.Version < expectedVersion
                   )
                   .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(50 * Math.Pow(i, 2)))
                   .ExecuteAsync(
                       ct => querySession.Query<ShoppingCartDetails>()
                           .SingleOrDefaultAsync(x => x.Id == query.CartId && x.Version >= expectedVersion, ct), token)
               ?? throw AggregateNotFoundException.For<ShoppingCart>(query.CartId);
    }
}
