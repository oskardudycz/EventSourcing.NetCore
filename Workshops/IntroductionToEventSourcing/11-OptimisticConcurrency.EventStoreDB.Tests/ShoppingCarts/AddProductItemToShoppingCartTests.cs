using Bogus;
using Ogooreck.API;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts.Scenarios;
using static OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts.Fixtures;

namespace OptimisticConcurrency.EventStoreDB.Tests.ShoppingCarts;

public class AddProductItemToShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [Trait("Category", "SkipCI")][InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                POST,
                URI(ShoppingCartProductItemsUrl(apiPrefix, ClientId, NotExistingShoppingCartId)),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(-1))
            )
            .Then(NOT_FOUND);

    [Theory]
    [Trait("Category", "SkipCI")][InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task AddsProductItemToEmptyShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(0))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(1));

    [Theory]
    [Trait("Category", "SkipCI")][InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task AddsProductItemToNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(1))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(2));

    [Theory]
    [Trait("Category", "SkipCI")][InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0),
                ThenConfirmed(apiPrefix, ClientId, 1)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(1))
            )
            .Then(CONFLICT);

    [Theory]
    [Trait("Category", "SkipCI")][InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0),
                ThenCanceled(apiPrefix, ClientId, 1)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(2))
            )
            .Then(CONFLICT);

    [Theory]
    [Trait("Category", "SkipCI")][InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task ReturnsNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 0)
            )
            .When(GET, URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK, RESPONSE_ETAG_HEADER(1));

    private static readonly Faker Faker = new();
    private readonly Guid NotExistingShoppingCartId = Guid.CreateVersion7();
    private readonly Guid ClientId = Guid.CreateVersion7();
    private readonly ProductItemRequest ProductItem = new(Guid.CreateVersion7(), Faker.Random.Number(1, 500));
}
