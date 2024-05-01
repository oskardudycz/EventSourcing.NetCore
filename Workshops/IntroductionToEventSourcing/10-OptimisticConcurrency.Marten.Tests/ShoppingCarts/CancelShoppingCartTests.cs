using Bogus;
using Ogooreck.API;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static OptimisticConcurrency.Marten.Tests.ShoppingCarts.Scenarios;
using static OptimisticConcurrency.Marten.Tests.ShoppingCarts.Fixtures;

namespace OptimisticConcurrency.Marten.Tests.ShoppingCarts;

public class CancelShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantCancelNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                DELETE,
                URI(ShoppingCartUrl(apiPrefix, ClientId, NotExistingShoppingCartId))
            )
            .Then(NOT_FOUND);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CancelsNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(2))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(3));

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantCancelAlreadyCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenCanceled(apiPrefix, ClientId)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(3))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantCancelConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenConfirmed(apiPrefix, ClientId)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                HEADERS(IF_MATCH(3))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task ReturnsNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenCanceled(apiPrefix, ClientId)
            )
            .When(GET, URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK, RESPONSE_ETAG_HEADER(3));

    private static readonly Faker Faker = new();
    private readonly Guid NotExistingShoppingCartId = Guid.NewGuid();
    private readonly Guid ClientId = Guid.NewGuid();
    private readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), Faker.Random.Number(1, 500));
}
