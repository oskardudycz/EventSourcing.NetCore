namespace Carts.Api.Requests.Carts;

public record RemoveProductRequest(
    PricedProductItemRequest? ProductItem
);
