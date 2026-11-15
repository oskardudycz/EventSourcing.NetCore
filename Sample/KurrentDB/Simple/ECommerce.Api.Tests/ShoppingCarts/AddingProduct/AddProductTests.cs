using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using Shouldly;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.AddingProduct;

using static ShoppingCartsApi;

public class AddProductTests(ApiFixture fixture): ApiTest(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Post_Should_AddProductItem_To_ShoppingCart()
    {
        var product = new ProductItemRequest(Guid.CreateVersion7(), 1);

        return API
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
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"), HEADERS(IF_MATCH(1)))
            .Until(RESPONSE_ETAG_IS(1))
            .Then(
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.ShouldBe(ctx.OpenedShoppingCartId());
                    details.Status.ShouldBe(ShoppingCartStatus.Pending);
                    var productItem = details.ProductItems.Single();
                    productItem.Quantity.ShouldBe(product.Quantity!.Value);
                    productItem.ProductId.ShouldBe(product.ProductId!.Value);
                    details.Version.ShouldBe(1);
                })
            );
    }
}
