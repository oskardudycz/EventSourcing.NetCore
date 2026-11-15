using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using Shouldly;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests(ApiFixture fixture): ApiTest(fixture)
{
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
                    .When(GET, URI($"/api/ShoppingCarts/{response.GetCreatedId()}"), HEADERS(IF_MATCH(0)))
                    .Until(RESPONSE_ETAG_IS(0))
                    .Then(
                        OK,
                        RESPONSE_BODY<ShoppingCartDetails>(details =>
                        {
                            details.Id.ShouldBe(response.GetCreatedId<Guid>());
                            details.Status.ShouldBe(ShoppingCartStatus.Pending);
                            details.ProductItems.ShouldBeEmpty();
                            details.ClientId.ShouldBe(ClientId);
                            details.Version.ShouldBe(0);
                        }))
        );

    public readonly Guid ClientId = Guid.CreateVersion7();
}
