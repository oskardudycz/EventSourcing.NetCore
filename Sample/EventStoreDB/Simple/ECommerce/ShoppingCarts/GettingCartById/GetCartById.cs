using Core.Validation;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace ECommerce.ShoppingCarts.GettingCartById;

public record GetCartById(
    Guid ShoppingCartId,
    int? ExpectedVersion
)
{
    public static GetCartById From(Guid cartId, int? expectedVersion) =>
        new(cartId.NotEmpty(), expectedVersion);

    public static Task<ShoppingCartDetails?> Handle(
        IQueryable<ShoppingCartDetails> shoppingCarts,
        GetCartById query,
        CancellationToken token
    )
    {
        var expectedVersion = query.ExpectedVersion;

        if (!expectedVersion.HasValue)
            return shoppingCarts.SingleOrDefaultAsync(x => x.Id == query.ShoppingCartId, token);

        return Policy
            .HandleResult<ShoppingCartDetails?>(cart =>
                cart == null || cart.Version < expectedVersion
            )
            .WaitAndRetryAsync(10, i => TimeSpan.FromMilliseconds(50 * Math.Pow(i, 2)))
            .ExecuteAsync(ct => shoppingCarts.SingleOrDefaultAsync(x => x.Id == query.ShoppingCartId && x.Version >= expectedVersion, ct), token);
}
