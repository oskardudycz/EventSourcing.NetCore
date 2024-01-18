using ECommerce.Domain.ShoppingCarts.Products;

namespace ECommerce.Domain.ShoppingCarts.AddingProduct;

public record AddProduct(
    Guid CartId,
    ProductItem ProductItem
);

public record ProductAdded(
    Guid CartId,
    PricedProductItem ProductItem
);
