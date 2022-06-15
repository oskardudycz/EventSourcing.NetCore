using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Core.Testing;
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
                .Then(CREATED),

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

    public readonly Guid ClientId = Guid.NewGuid();
}
