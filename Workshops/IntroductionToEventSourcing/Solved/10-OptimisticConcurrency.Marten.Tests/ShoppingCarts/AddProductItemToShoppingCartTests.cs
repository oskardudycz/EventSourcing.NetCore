using Bogus;
using Ogooreck.API;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static OptimisticConcurrency.Marten.Tests.ShoppingCarts.Scenarios;
using static OptimisticConcurrency.Marten.Tests.ShoppingCarts.Fixtures;

namespace OptimisticConcurrency.Marten.Tests.ShoppingCarts;

public class AddProductItemToShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                POST,
                URI(ShoppingCartProductItemsUrl(apiPrefix, ClientId, NotExistingShoppingCartId)),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(0))
            )
            .Then(NOT_FOUND);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task AddsProductItemToEmptyShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(1))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(2));

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task AddsProductItemToNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(2))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(3));

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1),
                ThenConfirmed(apiPrefix, ClientId, 2)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(3))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1),
                ThenCanceled(apiPrefix, ClientId, 2)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem)),
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
                WithProductItem(apiPrefix, ClientId, ProductItem, 1)
            )
            .When(GET, URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK, RESPONSE_ETAG_HEADER(2));

    private static readonly Faker Faker = new();
    private readonly Guid NotExistingShoppingCartId = Guid.CreateVersion7();
    private readonly Guid ClientId = Guid.CreateVersion7();
    private readonly ProductItemRequest ProductItem = new(Guid.CreateVersion7(), Faker.Random.Number(1, 500));
}
