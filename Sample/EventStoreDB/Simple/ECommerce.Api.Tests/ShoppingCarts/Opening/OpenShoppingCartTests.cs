using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests(ShoppingCartsApplicationFactory applicationFactory)
    : IClassFixture<ShoppingCartsApplicationFactory>
{
    private readonly ApiSpecification<Program> API = ApiSpecification<Program>.Setup(applicationFactory);

    [Fact]
    public Task Post_ShouldReturn_CreatedStatus_With_CartId() =>
        API.Scenario(
            API.Given()
                .When(
                    POST,
                    URI("/api/ShoppingCarts/"),
                    BODY(new OpenShoppingCartRequest(ClientId))
                )
                .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 0)),
            response =>
                API.Given()
                    .When(GET, URI($"/api/ShoppingCarts/{response.GetCreatedId()}"))
                    .Until(RESPONSE_ETAG_IS(0))
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

    public readonly Guid ClientId = Guid.NewGuid();
}
