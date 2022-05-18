using Carts.DataAccess;
using Carts.Pricing;
using Carts.ShoppingCarts.Products;

namespace Carts.ShoppingCarts.AddingProduct;

public class AddProductService: IAddProductService
{
    private readonly IProductPriceCalculator productPriceCalculator;
    private readonly IShoppingCartRepository shoppingCartRepository;

    public AddProductService(IProductPriceCalculator productPriceCalculator,
        IShoppingCartRepository shoppingCartRepository)
    {
        this.productPriceCalculator = productPriceCalculator;
        this.shoppingCartRepository = shoppingCartRepository;
    }

    public void AddProduct(Guid cartId, AddProductRequest model)
    {
        var cart = this.shoppingCartRepository.GetById(cartId);
        cart.AddProduct(this.productPriceCalculator, ProductItem.Create(model.ProductId, model.Quantity));
        this.shoppingCartRepository.Save(cart);
    }
}