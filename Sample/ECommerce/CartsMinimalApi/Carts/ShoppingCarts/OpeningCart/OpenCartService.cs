using Carts.DataAccess;

namespace Carts.ShoppingCarts.OpeningCart;

public class OpenCartService: IOpenCartService
{
    private readonly IShoppingCartRepository shoppingCartRepository;

    public OpenCartService(IShoppingCartRepository shoppingCartRepository)
    {
        this.shoppingCartRepository = shoppingCartRepository;
    }

    public Guid OpenCart(Guid clientId)
    {
        var cart = ShoppingCart.Open(Guid.NewGuid(), clientId);
        this.shoppingCartRepository.Save(cart);
        return cart.Id;
    }
}
