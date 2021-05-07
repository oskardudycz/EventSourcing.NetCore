using System;
using System.Collections.Generic;

namespace Orders.Api.Requests.Carts
{
    public record InitOrderRequest(
        Guid? ClientId,
        IReadOnlyList<PricedProductItemRequest>? ProductItems,
        decimal? TotalPrice
    );
}
