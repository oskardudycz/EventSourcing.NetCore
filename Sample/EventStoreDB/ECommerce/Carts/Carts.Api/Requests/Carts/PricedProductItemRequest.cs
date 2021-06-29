using System;

namespace Carts.Api.Requests.Carts
{
    public record PricedProductItemRequest(
        Guid? ProductId,
        int? Quantity,
        decimal? UnitPrice
    );
}
