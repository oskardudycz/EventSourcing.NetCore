namespace ECommerce.Domain.ShoppingCarts.Products;

public record PricedProductItem
{
    public Guid ProductId => ProductItem.ProductId;

    public int Quantity => ProductItem.Quantity;

    public decimal UnitPrice { get; }

    public decimal TotalPrice => Quantity * UnitPrice;
    public ProductItem ProductItem { get; }

    private PricedProductItem(ProductItem productItem, decimal unitPrice)
    {
        ProductItem = productItem;
        UnitPrice = unitPrice;
    }

    public static PricedProductItem Create(Guid? productId, int? quantity, decimal? unitPrice) =>
        Create(
            ProductItem.From(productId, quantity),
            unitPrice
        );

    public static PricedProductItem Create(ProductItem productItem, decimal? unitPrice) =>
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

    public PricedProductItem Subtract(PricedProductItem pricedProductItem)
    {
        if (!MatchesProductAndPrice(pricedProductItem))
            throw new ArgumentException("Product or price does not match.");

        return new PricedProductItem(ProductItem.Subtract(pricedProductItem.ProductItem), UnitPrice);
    }

    public bool HasEnough(int quantity) =>
        ProductItem.HasEnough(quantity);

    public bool HasTheSameQuantity(PricedProductItem pricedProductItem) =>
        ProductItem.HasTheSameQuantity(pricedProductItem.ProductItem);
}
