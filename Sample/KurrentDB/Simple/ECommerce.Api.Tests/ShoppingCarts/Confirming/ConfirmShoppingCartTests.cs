using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using Shouldly;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.Confirming;

using static ShoppingCartsApi;

public class ConfirmShoppingCartTests(ApiFixture fixture): ApiTest(fixture)
{
    private Guid ClientId = Guid.CreateVersion7();

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Return_OK_And_Confirm_Shopping_Cart() =>
        await API
            .Given("Opened Shopping Cart", OpenShoppingCart(ClientId))
            .When(
                PUT,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/confirmation"),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK)
            .AndWhen(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"), HEADERS(IF_MATCH(1)))
            .Until(RESPONSE_ETAG_IS(1))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>((details,ctx) =>
                {
                    details.Id.ShouldBe(ctx.OpenedShoppingCartId());
                    details.Status.ShouldBe(ShoppingCartStatus.Confirmed);
                    details.ProductItems.ShouldBeEmpty();
                    details.ClientId.ShouldBe(ClientId);
                    details.Version.ShouldBe(1);
                }));
    // API.PublishedExternalEventsOfType<CartFinalized>();
}
