namespace Carts.ShoppingCarts.Products;

public record PricedProductItem(
    ProductItem ProductItem,
    decimal UnitPrice
)
{
    public Guid ProductId => ProductItem.ProductId;
    public int Quantity => ProductItem.Quantity;

    public decimal TotalPrice => Quantity * UnitPrice;

    public static PricedProductItem From(ProductItem productItem, decimal? unitPrice) =>
        unitPrice switch
        {
            null => throw new ArgumentNullException(nameof(unitPrice)),
            <= 0 => throw new ArgumentOutOfRangeException(nameof(unitPrice),
                "Unit price has to be positive number"),
            _ => new PricedProductItem(productItem, unitPrice.Value)
        };

    public bool MatchesProductAndPrice(PricedProductItem pricedProductItem) =>
        ProductId == pricedProductItem.ProductId && UnitPrice == pricedProductItem.UnitPrice;

    public PricedProductItem MergeWith(PricedProductItem pricedProductItem)
    {
        if (!MatchesProductAndPrice(pricedProductItem))
            throw new ArgumentException("Product or price does not match.");

        return new PricedProductItem(ProductItem.MergeWith(pricedProductItem.ProductItem), UnitPrice);
    }

    public PricedProductItem Substract(PricedProductItem pricedProductItem)
    {
        if (!MatchesProductAndPrice(pricedProductItem))
            throw new ArgumentException("Product or price does not match.");

        return new PricedProductItem(ProductItem.Substract(pricedProductItem.ProductItem), UnitPrice);
    }

    public bool HasEnough(int quantity) =>
        ProductItem.HasEnough(quantity);

    public bool HasTheSameQuantity(PricedProductItem pricedProductItem) =>
        ProductItem.HasTheSameQuantity(pricedProductItem.ProductItem);
}
