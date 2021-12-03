using System;

namespace Carts.Api.Requests.Carts;

public record ProductItemRequest(
    Guid ProductId,
    int Quantity
);
