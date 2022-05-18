using Carts.DataAccess;

namespace Carts.ShoppingCarts.ConfirmingCart;

public class ConfirmCartService: IConfirmCartService
{
    private readonly IShoppingCartRepository shoppingCartRepository;

    public ConfirmCartService(IShoppingCartRepository shoppingCartRepository)
    {
        this.shoppingCartRepository = shoppingCartRepository;
    }

    public void Confirm(Guid cartId)
    {
        var cart = this.shoppingCartRepository.GetById(cartId);
        cart.Confirm();
        this.shoppingCartRepository.Save(cart);
    }
}