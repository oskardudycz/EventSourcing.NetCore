using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Shouldly;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Confirming;

using static ShoppingCartsApi;

public class ConfirmShoppingCartTests(ApiFixture fixture): ApiTest(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Return_OK_And_Cancel_Shopping_Cart()
    {
        await API
            .Given(OpenShoppingCart(ClientId))
            .When(
                PUT,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/confirmation"),
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
                    details.Status.ShouldBe(ShoppingCartStatus.Confirmed);
                    details.ProductItems.ShouldBeEmpty();
                    details.ClientId.ShouldBe(ClientId);
                    details.Version.ShouldBe(1);
                }));

        // API.PublishedExternalEventsOfType<CartFinalized>();
    }

    private readonly Guid ClientId = Guid.CreateVersion7();
}
