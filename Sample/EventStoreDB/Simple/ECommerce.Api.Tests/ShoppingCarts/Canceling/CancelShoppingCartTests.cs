using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static ECommerce.Api.Tests.ShoppingCarts.ShoppingCartsApi;

namespace ECommerce.Api.Tests.ShoppingCarts.Canceling;

public class CancelShoppingCartTests(ApiSpecification<Program> api): IClassFixture<ApiSpecification<Program>>
{
    public readonly Guid ClientId = Guid.NewGuid();

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart() =>
        api
            .Given("Opened Shopping Cart", OpenShoppingCart(ClientId))
            .When(
                "Cancel Shopping Cart",
                DELETE,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"))
            .Until(RESPONSE_ETAG_IS(1), 10)
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.Should().Be(ctx.OpenedShoppingCartId());
                    details.Status.Should().Be(ShoppingCartStatus.Canceled);
                    details.ProductItems.Should().BeEmpty();
                    details.ClientId.Should().Be(ClientId);
                    details.Version.Should().Be(1);
                }));
}
