namespace Carts.ShoppingCarts.Products;

public record ProductItem
{
    public Guid ProductId { get; }

    public int Quantity { get; }

    private ProductItem(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public static ProductItem From(Guid? productId, int? quantity)
    {
        if (!productId.HasValue)
            throw new ArgumentNullException(nameof(productId));

        return quantity switch
        {
            null => throw new ArgumentNullException(nameof(quantity)),
            <= 0 => throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity has to be a positive number"),
            _ => new ProductItem(productId.Value, quantity.Value)
        };
    }

    public ProductItem MergeWith(ProductItem productItem)
    {
        if (!MatchesProduct(productItem))
            throw new ArgumentException("Product does not match.");

        return From(ProductId, Quantity + productItem.Quantity);
    }

    public ProductItem Subtract(ProductItem productItem)
    {
        if (!MatchesProduct(productItem))
            throw new ArgumentException("Product does not match.");

        return From(ProductId, Quantity - productItem.Quantity);
    }

    public bool MatchesProduct(ProductItem productItem)
    {
        return ProductId == productItem.ProductId;
    }

    public bool HasEnough(int quantity)
    {
        return Quantity >= quantity;
    }

    public bool HasTheSameQuantity(ProductItem productItem)
    {
        return Quantity == productItem.Quantity;
    }
}
