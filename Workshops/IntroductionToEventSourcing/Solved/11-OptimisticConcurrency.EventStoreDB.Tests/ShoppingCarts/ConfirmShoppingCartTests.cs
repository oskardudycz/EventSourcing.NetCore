using Bogus;
using Ogooreck.API;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts.Scenarios;
using static OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts.Fixtures;

namespace OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts;

public class ConfirmShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantConfirmNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                POST,
                URI(ConfirmShoppingCartUrl(apiPrefix, ClientId, NotExistingShoppingCartId)),
                HEADERS(IF_MATCH(-1))
            )
            .Then(NOT_FOUND);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantConfirmEmptyShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(
                POST,
                URI(ctx => ConfirmShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(0))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task ConfirmsNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0)
            )
            .When(
                POST,
                URI(ctx => ConfirmShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(1))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(2));

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantConfirmAlreadyConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0),
                ThenConfirmed(apiPrefix, ClientId, 1)
            )
            .When(
                POST,
                URI(ctx => ConfirmShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(2))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantConfirmCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0),
                ThenCanceled(apiPrefix, ClientId, 1)
            )
            .When(
                POST,
                URI(ctx => ConfirmShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(2))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task ReturnsNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0),
                ThenConfirmed(apiPrefix, ClientId, 1)
            )
            .When(GET, URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK, RESPONSE_ETAG_HEADER(2));

    private static readonly Faker Faker = new();
    private readonly Guid NotExistingShoppingCartId = Guid.NewGuid();
    private readonly Guid ClientId = Guid.NewGuid();
    private readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), Faker.Random.Number(1, 500));
}
