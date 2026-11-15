using System.Text.Json;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.Products;
using Carts.Tests.Stubs.Products;
using Shouldly;
using Xunit;

namespace Carts.Tests.Carts;

public class CartSerializationTests
{
    [Fact]
    public void SnapshotRoundTripPreservesCartAndProductValueObjects()
    {
        var cart = ShoppingCart.Open(Guid.NewGuid(), Guid.NewGuid());
        cart.AddProduct(new FakeProductPriceCalculator(), ProductItem.From(Guid.NewGuid(), 2));
        cart.Confirm();

        var restored = JsonSerializer.Deserialize<ShoppingCart>(JsonSerializer.Serialize(cart))!;

        restored.Id.ShouldBe(cart.Id);
        restored.ClientId.ShouldBe(cart.ClientId);
        restored.Status.ShouldBe(cart.Status);
        restored.ProductItems.ShouldBeEquivalentTo(cart.ProductItems);
        restored.TotalPrice.ShouldBe(cart.TotalPrice);
        restored.DequeueUncommittedEvents().ShouldBeEmpty();
    }
}
