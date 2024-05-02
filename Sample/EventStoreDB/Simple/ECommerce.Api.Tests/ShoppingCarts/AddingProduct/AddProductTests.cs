using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.AddingProduct;

using static ShoppingCartsApi;

public class AddProductTests(ApiSpecification<Program> api): IClassFixture<ApiSpecification<Program>>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Post_Should_AddProductItem_To_ShoppingCart()
    {
        var product = new ProductItemRequest(Guid.NewGuid(), 1);

        return api
            .Given("Opened Shopping Cart", OpenShoppingCart())
            .When(
                "Add new product",
                POST,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/products"),
                BODY(new AddProductRequest(product)),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"))
            .Until(RESPONSE_ETAG_IS(1))
            .Then(
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.Should().Be(ctx.OpenedShoppingCartId());
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    var productItem = details.ProductItems.Single();
                    productItem.Quantity.Should().Be(product.Quantity);
                    productItem.ProductId.Should().Be(product.ProductId!.Value);
                    details.Version.Should().Be(1);
                })
            );
    }
}
