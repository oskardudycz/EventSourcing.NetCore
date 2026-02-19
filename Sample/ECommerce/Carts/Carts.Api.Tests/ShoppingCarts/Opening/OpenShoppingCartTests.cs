using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests(ApiFixture fixture): ApiTest(fixture)
{
    [Fact]
    public Task Post_ShouldReturn_CreatedStatus_With_CartId() =>
        API.Given()
            .When(
                POST,
                URI("/api/ShoppingCarts/"),
                BODY(new OpenShoppingCartRequest(ClientId))
            )
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1))
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.GetCreatedId()}"), HEADERS(IF_MATCH(1)))
            .Until(RESPONSE_ETAG_IS(1))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.Should().Be(ctx.GetCreatedId<Guid>());
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    details.ProductItems.Should().BeEmpty();
                    details.ClientId.Should().Be(ClientId);
                    details.Version.Should().Be(1);
                }));

    public readonly Guid ClientId = Guid.CreateVersion7();
}
