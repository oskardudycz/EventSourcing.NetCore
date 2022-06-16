using Core.Testing;
using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using ECommerce.ShoppingCarts.ProductItems;
using FluentAssertions;
using Ogooreck.API;
using Warehouse.Api.Tests;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests: IClassFixture<ShoppingCartsApplicationFactory>
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
                .Then(CREATED),

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

    public OpenShoppingCartTests(ShoppingCartsApplicationFactory applicationFactory) =>
        API = ApiSpecification<Program>.Setup(applicationFactory);

    public readonly Guid ClientId = Guid.NewGuid();
}
