using Carts.Pricing;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.Products;
using Carts.Tests.Stubs.Products;
using Core.Aggregates;

namespace Carts.Tests.Builders;

internal class CartBuilder
{
    private readonly IProductPriceCalculator productPriceCalculator = new FakeProductPriceCalculator();
    private Func<ShoppingCart> build = () => new ShoppingCart();
    private Func<ShoppingCart, ShoppingCart>? modify;

    public CartBuilder Opened()
    {
        var cartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();

        // When
        var cart = ShoppingCart.Open(
            cartId,
            clientId
        );

        build = () => cart;

        return this;
    }

    public CartBuilder WithProduct()
    {
        var productId = Guid.CreateVersion7();
        const int quantity = 1;
        modify += cart =>
        {
            cart.AddProduct(productPriceCalculator, ProductItem.From(productId, quantity));
            return cart;
        };
        return this;
    }

    public static CartBuilder Create() => new();

    public ShoppingCart Build()
    {
        var cart = build();
        modify?.Invoke(cart);
        ((IAggregate)cart).DequeueUncommittedEvents();
        return cart;
    }
}
