using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static ApplicationLogic.Marten.Tests.Incidents.Scenarios;
using static ApplicationLogic.Marten.Tests.Incidents.Fixtures;

namespace ApplicationLogic.Marten.Tests.Incidents;

public class OpenShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task OpensShoppingCart(string apiPrefix) =>
        api.Given()
            .When(POST, URI(ShoppingCarts(apiPrefix, ClientId)))
            .Then(CREATED_WITH_DEFAULT_HEADERS(locationHeaderPrefix: ShoppingCarts(apiPrefix, ClientId)));


    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task ReturnsOpenedShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(GET, URI(ctx => ShoppingCart(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK);

    private readonly Guid ClientId = Guid.NewGuid();
}
