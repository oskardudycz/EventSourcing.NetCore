using ApplicationLogic.EventStoreDB.Immutable.ShoppingCarts;
using Bogus;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static ApplicationLogic.EventStoreDB.Tests.ShoppingCarts.Scenarios;
using static ApplicationLogic.EventStoreDB.Tests.ShoppingCarts.Fixtures;

namespace ApplicationLogic.EventStoreDB.Tests.ShoppingCarts;

public class AddProductItemToShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                POST,
                URI(ShoppingCartProductItemsUrl(apiPrefix, ClientId, NotExistingShoppingCartId)),
                BODY(new AddProductRequest(ProductItem))
            )
            .Then(NOT_FOUND);

    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task AddsProductItemToEmptyShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem))
            )
            .Then(NO_CONTENT);


    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task AddsProductItemToNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem))
            )
            .Then(NO_CONTENT);


    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenConfirmed(apiPrefix, ClientId)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem))
            )
            .Then(CONFLICT);


    [Theory]
    [Trait("Category", "SkipCI")]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantAddProductItemToCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenCanceled(apiPrefix, ClientId)
            )
            .When(
                POST,
                URI(ctx => ShoppingCartProductItemsUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())),
                BODY(new AddProductRequest(ProductItem))
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
                WithProductItem(apiPrefix, ClientId, ProductItem)
            )
            .When(GET, URI(ctx => ShoppingCartUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>())))
            .Then(OK);

    private static readonly Faker Faker = new();
    private readonly Guid NotExistingShoppingCartId = Guid.NewGuid();
    private readonly Guid ClientId = Guid.NewGuid();
    private readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), Faker.Random.Number(1, 500));
}
