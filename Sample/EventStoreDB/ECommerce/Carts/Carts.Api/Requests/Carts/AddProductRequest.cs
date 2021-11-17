using System;

namespace Carts.Api.Requests.Carts;

public record AddProductRequest(
    Guid? CartId,
    ProductItemRequest? ProductItem
);