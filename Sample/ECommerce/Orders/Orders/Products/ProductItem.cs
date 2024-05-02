namespace Orders.Products;

public class ProductItem(Guid productId, int quantity)
{
    public Guid ProductId { get; } = productId;

    public int Quantity { get; } = quantity;
}
