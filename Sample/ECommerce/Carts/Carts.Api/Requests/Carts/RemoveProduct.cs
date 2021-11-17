using System;

namespace Carts.Api.Requests.Carts;

public record RemoveProductRequest(
    Guid? CartId,
    PricedProductItemRequest? ProductItem
);