using Carts.Api.Requests;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts;

public class ShoppingCartsApi: IClassFixture<ApiSpecification<Program>>
{
    private ApiSpecification<Program> API;

    public ShoppingCartsApi(ApiSpecification<Program> api) => API = api;

    [Fact]
    public Task Post_ShouldOpenShoppingCart() =>
        API.Given(
                URI("/api/ShoppingCarts"),
                BODY(new OpenShoppingCartRequest(Guid.NewGuid()))
            )
            .When(POST)
            .Then(
                CREATED
            );
}
