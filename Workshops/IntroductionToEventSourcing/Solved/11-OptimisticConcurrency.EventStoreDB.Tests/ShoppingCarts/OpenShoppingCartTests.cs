using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts.Scenarios;
using static OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts.Fixtures;

namespace OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts;

public class OpenShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task OpensShoppingCart(string apiPrefix) =>
        api.Given()
            .When(POST, URI(ShoppingCartsUrl(apiPrefix, ClientId)))
            .Then(
                CREATED_WITH_DEFAULT_HEADERS(locationHeaderPrefix: ShoppingCartsUrl(apiPrefix, ClientId)),
                RESPONSE_ETAG_HEADER(0)
            );

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task ReturnsOpenedShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(GET, URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK, RESPONSE_ETAG_HEADER(0));

    private readonly Guid ClientId = Guid.CreateVersion7();
}
