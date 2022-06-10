using Carts.DataAccess;
using Carts.ShoppingCarts.Products;

namespace Carts.ShoppingCarts.RemovingProduct;

public class RemoveProductService: IRemoveProductService
{
    private readonly IShoppingCartRepository shoppingCartRepository;

    public RemoveProductService(IShoppingCartRepository shoppingCartRepository)
    {
        this.shoppingCartRepository = shoppingCartRepository;
    }

    public void RemoveProduct(Guid cartId, Guid productId, int? quantity, decimal? unitPrice)
    {
        var pricedProduct = PricedProductItem.Create(productId, quantity, unitPrice);
        var cart = this.shoppingCartRepository.GetById(cartId);
        cart.RemoveProduct(pricedProduct);
        this.shoppingCartRepository.Save(cart);
    }
}