namespace Carts.ShoppingCarts;

public record OpenShoppingCartRequest(
    Guid ClientId
);

public record AddProductRequest(
    Guid ProductId,
    int Quantity
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

public record GetCartAtVersionRequest(
    Guid? CartId,
    long? Version
);
