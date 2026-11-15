using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Shouldly;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.AddingProduct;

using static ShoppingCartsApi;

public class AddProductTests(ApiFixture fixture): ApiTest(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Post_Should_AddProductItem_To_ShoppingCart() =>
        API
            .Given("Opened Shopping Cart", OpenShoppingCart())
            .When(
                "Add new product",
                POST,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/products"),
                BODY(new AddProductRequest(product)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"), HEADERS(IF_MATCH(2)))
            .Until(RESPONSE_ETAG_IS(2))
            .Then(
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.ShouldBe(ctx.OpenedShoppingCartId());
                    details.Status.ShouldBe(ShoppingCartStatus.Pending);
                    var productItem = details.ProductItems.Single();
                    productItem.Quantity.ShouldBe(product.Quantity!.Value);
                    productItem.ProductId.ShouldBe(product.ProductId!.Value);
                    details.Version.ShouldBe(2);
                })
            );

    private readonly ProductItemRequest product = new(Guid.CreateVersion7(), 1);
}
