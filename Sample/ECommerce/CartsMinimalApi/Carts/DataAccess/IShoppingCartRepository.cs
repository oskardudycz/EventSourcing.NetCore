namespace Carts.DataAccess;

public interface IShoppingCartRepository
{
    ShoppingCart GetById(Guid id);
    void Save(ShoppingCart cart);
}