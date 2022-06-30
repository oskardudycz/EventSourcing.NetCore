using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Core.Testing;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests: IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly ApiSpecification<Program> API;

    [Fact]
    public Task Post_ShouldReturn_CreatedStatus_With_CartId() =>
        API.Scenario(
            API.Given(
                    URI("/api/ShoppingCarts/"),
                    BODY(new OpenShoppingCartRequest(ClientId))
                )
                .When(POST)
                .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 0)),

            response =>
                API.Given(
                        URI($"/api/ShoppingCarts/{response.GetCreatedId()}")
                    )
                    .When(GET_UNTIL(RESPONSE_ETAG_IS(0)))
                    .Then(
                        OK,
                        RESPONSE_BODY<ShoppingCartDetails>(details =>
                        {
                            details.Id.Should().Be(response.GetCreatedId<Guid>());
                            details.Status.Should().Be(ShoppingCartStatus.Pending);
                            details.ProductItems.Should().BeEmpty();
                            details.ClientId.Should().Be(ClientId);
                            details.Version.Should().Be(0);
                        }))
        );

    public OpenShoppingCartTests(TestWebApplicationFactory<Program> fixture) =>
        API = ApiSpecification<Program>.Setup(fixture);

    public readonly Guid ClientId = Guid.NewGuid();
}
