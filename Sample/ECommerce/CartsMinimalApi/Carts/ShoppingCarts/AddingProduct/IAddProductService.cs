namespace Carts.ShoppingCarts.AddingProduct;

public interface IAddProductService
{
    void AddProduct(Guid cartId, Guid productId, int quantity);
}
