using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ShoppingCarts.GettingCartById
{
    public record GetCartById(
        Guid CartId
    )
    {
        public static GetCartById From(Guid cartId)
        {
            if (cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new GetCartById(cartId);
        }

        public static Task<ShoppingCartDetails> Handle(
            IQueryable<ShoppingCartDetails> shoppingCarts,
            GetCartById query,
            CancellationToken ct
        )
        {
            return shoppingCarts
                .SingleOrDefaultAsync(
                    x => x.Id == query.CartId, ct
                );
        }
    }
}
