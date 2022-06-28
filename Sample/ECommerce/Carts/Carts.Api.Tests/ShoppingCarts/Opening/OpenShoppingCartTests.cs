using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Core.Testing;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartTests: IClassFixture<TestWebApplicationFactory<Program>>
{
    [Fact]
    public Task Post_ShouldReturn_CreatedStatus_With_CartId() =>
        API.Scenario(
            API.Given(
                    URI("/api/ShoppingCarts/"),
                    BODY(new OpenShoppingCartRequest(ClientId))
                )
                .When(POST)
                .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1)),

            response =>
                API.Given(
                        URI($"/api/ShoppingCarts/{response.GetCreatedId()}")
                    )
                    .When(GET_UNTIL(RESPONSE_ETAG_IS(1)))
                    .Then(
                        OK,
                        RESPONSE_BODY(new ShoppingCartDetails
                        {
                            Id = response.GetCreatedId<Guid>(),
                            Status = ShoppingCartStatus.Pending,
                            ProductItems = new List<PricedProductItem>(),
                            ClientId = ClientId,
                            Version = 1,
                        }))
        );

    public OpenShoppingCartTests(TestWebApplicationFactory<Program> fixture) =>
        API = ApiSpecification<Program>.Setup(fixture);

    private readonly ApiSpecification<Program> API;
    private readonly Guid ClientId = Guid.NewGuid();
}
