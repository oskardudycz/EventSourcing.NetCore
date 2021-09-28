using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ShoppingCarts.GettingCarts
{
    public record GetCarts(
        int PageNumber,
        int PageSize
    )
    {
        public static GetCarts From(int? pageNumber = 1, int? pageSize = 20)
        {
            if (pageNumber is null or <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize is null or <= 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            return new GetCarts(pageNumber.Value, pageSize.Value);
        }

        public static async Task<IReadOnlyList<ShoppingCartShortInfo>> Handle(
            IQueryable<ShoppingCartShortInfo> shoppingCarts,
            GetCarts query,
            CancellationToken ct
        )
        {
            var (page, pageSize) = query;

            return await shoppingCarts
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync(ct);
        }
    }
}
