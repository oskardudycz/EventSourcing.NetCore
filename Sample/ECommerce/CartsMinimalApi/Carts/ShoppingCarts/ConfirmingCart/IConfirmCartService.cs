namespace Carts.ShoppingCarts.ConfirmingCart;

public interface IConfirmCartService
{
    void Confirm(Guid cartId);
}