namespace Carts.ShoppingCarts.OpeningCart;

public interface IOpenCartService
{
    Guid OpenCart(OpenShoppingCartRequest openShoppingCartRequest);
}