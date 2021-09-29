using System;

namespace ECommerce.Api.Requests
{
    public record InitializeShoppingCartRequest(
        Guid? ClientId
    );

    public class ProductItemRequest
    {
        public Guid ProductId { get; set; }

        public int Quantity { get;  set; }
    }

    public record AddProductRequest(
        Guid? CartId,
        ProductItemRequest? ProductItem
    );

    public record PricedProductItemRequest(
        Guid? ProductId,
        int? Quantity,
        decimal? UnitPrice
    );

    public record RemoveProductRequest(
        Guid? CartId,
        PricedProductItemRequest? ProductItem
    );
}
