using Carts.DataAccess;

namespace Carts.ShoppingCarts.OpeningCart;

public class OpenCartService: IOpenCartService
{
    private readonly IShoppingCartRepository shoppingCartRepository;

    public OpenCartService(IShoppingCartRepository shoppingCartRepository)
    {
        this.shoppingCartRepository = shoppingCartRepository;
    }

    public Guid OpenCart(OpenShoppingCartRequest openShoppingCartRequest)
    {
        var cart = ShoppingCart.Open(Guid.NewGuid(), openShoppingCartRequest.ClientId);
        this.shoppingCartRepository.Save(cart);
        return cart.Id;
    }
}