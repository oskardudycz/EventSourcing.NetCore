using System;

namespace ECommerce.Api.Requests;

public record InitializeShoppingCartRequest(
    Guid? ClientId
);

public record ProductItemRequest(
    Guid? ProductId,
    int? Quantity
);

public record AddProductRequest(
    ProductItemRequest? ProductItem
);

public record PricedProductItemRequest(
    Guid? ProductId,
    int? Quantity,
    decimal? UnitPrice
);

public record RemoveProductRequest(
    PricedProductItemRequest? ProductItem
);

public record ConfirmShoppingCartRequest;
