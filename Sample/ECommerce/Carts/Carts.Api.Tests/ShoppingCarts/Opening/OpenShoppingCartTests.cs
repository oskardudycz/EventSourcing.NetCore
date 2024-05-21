using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests(ApiSpecification<Program> api): IClassFixture<ApiSpecification<Program>>
{
    [Fact]
    public Task Post_ShouldReturn_CreatedStatus_With_CartId() =>
        api.Scenario(
            api.Given()
                .When(
                    POST,
                    URI("/api/ShoppingCarts/"),
                    BODY(new OpenShoppingCartRequest(ClientId))
                )
                .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1)),
            response =>
                api.Given()
                    .When(GET, URI($"/api/ShoppingCarts/{response.GetCreatedId()}"))
                    .Until(RESPONSE_ETAG_IS(1), 10)
                    .Then(
                        OK,
                        RESPONSE_BODY<ShoppingCartDetails>(details =>
                        {
                            details.Id.Should().Be(response.GetCreatedId<Guid>());
                            details.Status.Should().Be(ShoppingCartStatus.Pending);
                            details.ProductItems.Should().BeEmpty();
                            details.ClientId.Should().Be(ClientId);
                            details.Version.Should().Be(1);
                        }))
        );

    public readonly Guid ClientId = Guid.NewGuid();
}
