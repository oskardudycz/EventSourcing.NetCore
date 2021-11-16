using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ShoppingCarts.GettingCartById
{
    public record GetCartById(
        Guid ShoppingCartId
    )
    {
        public static GetCartById From(Guid cartId)
        {
            if (cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new GetCartById(cartId);
        }

        public static async Task<ShoppingCartDetails> Handle(
            IQueryable<ShoppingCartDetails> shoppingCarts,
            GetCartById query,
            CancellationToken ct
        )
        {
            return await shoppingCarts
                .SingleOrDefaultAsync(
                    x => x.Id == query.ShoppingCartId, ct
                ) ?? throw new InvalidOperationException();
        }
    }
}
