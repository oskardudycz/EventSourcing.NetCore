using System.Text.Json;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.Products;
using Carts.Tests.Stubs.Products;
using FluentAssertions;
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

        restored.Id.Should().Be(cart.Id);
        restored.ClientId.Should().Be(cart.ClientId);
        restored.Status.Should().Be(cart.Status);
        restored.ProductItems.Should().BeEquivalentTo(cart.ProductItems);
        restored.TotalPrice.Should().Be(cart.TotalPrice);
        restored.DequeueUncommittedEvents().Should().BeEmpty();
    }
}
