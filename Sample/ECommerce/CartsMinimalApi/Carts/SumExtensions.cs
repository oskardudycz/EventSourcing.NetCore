using Carts.ShoppingCarts.Products;

namespace Carts;

public static class SumExtensions
{
    public static Money Sum<T>(this IEnumerable<T> source, Func<T, Money> selector)
    {
        return source.Select(selector).Aggregate((x, y) => x + y);
    }
}
