namespace Orders.Products;

public class PricedProductItem
{
    public Guid ProductId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    private PricedProductItem(Guid productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static PricedProductItem Create(Guid? productId, int? quantity, decimal? unitPrice) =>
        new (
            productId ?? throw new ArgumentNullException(nameof(productId)),
            quantity ?? throw new ArgumentNullException(nameof(quantity)),
            unitPrice ?? throw new ArgumentNullException(nameof(unitPrice))
        );
}
