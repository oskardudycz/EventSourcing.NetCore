using Bogus;
using Ogooreck.API;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static OptimisticConcurrency.Marten.Tests.ShoppingCarts.Scenarios;
using static OptimisticConcurrency.Marten.Tests.ShoppingCarts.Fixtures;

namespace OptimisticConcurrency.Marten.Tests.ShoppingCarts;

public class RemoveProductItemFromShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveProductItemFromNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                DELETE,
                URI(ShoppingCartProductItemUrl(apiPrefix, ClientId, NotExistingShoppingCartId, ProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(0))
            )
            .Then(NOT_FOUND);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveProductItemFromEmptyShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(1))
            )
            .Then(CONFLICT);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task RemovesExistingProductItemFromShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(2))
            )
            .Then(NO_CONTENT, RESPONSE_ETAG_HEADER(3));

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveExistingProductItemFromShoppingCartWithWrongETag(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(),
                    ProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(1))
            )
            .Then(PRECONDITION_FAILED);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveExistingProductItemFromShoppingCartWithoutETag(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(),
                    ProductItem.ProductId!.Value))
            )
            .Then(PRECONDITION_FAILED);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveNonExistingProductItemFromNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), NotExistingProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(2))
            )
            .Then(CONFLICT);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveExistingProductItemFromCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1),
                ThenCanceled(apiPrefix, ClientId, 2)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(3))
            )
            .Then(CONFLICT);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveExistingProductItemFromConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem, 1),
                ThenConfirmed(apiPrefix, ClientId, 2)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value)),
                HEADERS(IF_MATCH(3))
            )
            .Then(CONFLICT);

    [Theory]
    [Trait("Category", "SkipCI")]
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
    private readonly Guid NotExistingShoppingCartId = Guid.NewGuid();
    private readonly Guid ClientId = Guid.NewGuid();
    private readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), Faker.Random.Number(1, 500));
    private readonly ProductItemRequest NotExistingProductItem = new(Guid.NewGuid(), 1);
}
