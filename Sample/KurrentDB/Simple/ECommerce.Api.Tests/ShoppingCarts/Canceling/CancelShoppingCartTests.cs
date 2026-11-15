using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using Shouldly;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static ECommerce.Api.Tests.ShoppingCarts.ShoppingCartsApi;

namespace ECommerce.Api.Tests.ShoppingCarts.Canceling;

public class CancelShoppingCartTests(ApiFixture fixture): ApiTest(fixture)
{
    public readonly Guid ClientId = Guid.CreateVersion7();

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart() =>
        API
            .Given("Opened Shopping Cart", OpenShoppingCart(ClientId))
            .When(
                "Cancel Shopping Cart",
                DELETE,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"), HEADERS(IF_MATCH(1)))
            .Until(RESPONSE_ETAG_IS(1))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.ShouldBe(ctx.OpenedShoppingCartId());
                    details.Status.ShouldBe(ShoppingCartStatus.Canceled);
                    details.ProductItems.ShouldBeEmpty();
                    details.ClientId.ShouldBe(ClientId);
                    details.Version.ShouldBe(1);
                }));
}
