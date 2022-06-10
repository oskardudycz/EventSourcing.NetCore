namespace Carts.ShoppingCarts.CancellingCart;

public interface ICancelCartService
{
    void CancelCart(Guid cartId);
}