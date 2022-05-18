using Carts.DataAccess;

namespace Carts.ShoppingCarts.CancellingCart;

public class CancelCartService: ICancelCartService
{
    private readonly IShoppingCartRepository shoppingCartRepository;

    public CancelCartService(IShoppingCartRepository shoppingCartRepository)
    {
        this.shoppingCartRepository = shoppingCartRepository;
    }

    public void CancelCart(Guid cartId)
    {
        var cart = this.shoppingCartRepository.GetById(cartId);
        cart.Cancel();
        this.shoppingCartRepository.Save(cart);
    }
}