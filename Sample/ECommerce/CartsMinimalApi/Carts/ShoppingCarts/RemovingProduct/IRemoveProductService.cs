namespace Carts.ShoppingCarts.RemovingProduct;

public interface IRemoveProductService
{
    void RemoveProduct(Guid cartId, Guid productId, int? quantity, decimal? unitPrice);
}