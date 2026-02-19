using ApplicationLogic.EventStoreDB.Immutable.ShoppingCarts;
using Bogus;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;
using static ApplicationLogic.EventStoreDB.Tests.ShoppingCarts.Scenarios;
using static ApplicationLogic.EventStoreDB.Tests.ShoppingCarts.Fixtures;

namespace ApplicationLogic.EventStoreDB.Tests.ShoppingCarts;

public class RemoveProductItemFromShoppingCartTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveProductItemFromNotExistingShoppingCart(string apiPrefix) =>
        api.Given()
            .When(
                DELETE,
                URI(ShoppingCartProductItemUrl(apiPrefix, ClientId, NotExistingShoppingCartId, ProductItem.ProductId!.Value))
            )
            .Then(NOT_FOUND);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveProductItemFromEmptyShoppingCart(string apiPrefix) =>
        api.Given(OpenedShoppingCart(apiPrefix, ClientId))
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task RemovesExistingProductItemFromShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value))
            )
            .Then(NO_CONTENT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveNonExistingProductItemFromNonEmptyShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), NotExistingProductItem.ProductId!.Value))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveExistingProductItemFromCanceledShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenCanceled(apiPrefix, ClientId)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value))
            )
            .Then(CONFLICT);

    [Theory]
    [InlineData("immutable")]
    [InlineData("mutable")]
    [InlineData("mixed")]
    public Task CantRemoveExistingProductItemFromConfirmedShoppingCart(string apiPrefix) =>
        api.Given(
                OpenedShoppingCart(apiPrefix, ClientId),
                WithProductItem(apiPrefix, ClientId, ProductItem),
                ThenConfirmed(apiPrefix, ClientId)
            )
            .When(
                DELETE,
                URI(ctx => ShoppingCartProductItemUrl(apiPrefix, ClientId, ctx.GetCreatedId<Guid>(), ProductItem.ProductId!.Value))
            )
            .Then(CONFLICT);

    [Theory]
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
    private readonly Guid NotExistingShoppingCartId = Guid.CreateVersion7();
    private readonly Guid ClientId = Guid.CreateVersion7();
    private readonly ProductItemRequest ProductItem = new(Guid.CreateVersion7(), Faker.Random.Number(1, 500));
    private readonly ProductItemRequest NotExistingProductItem = new(Guid.CreateVersion7(), 1);
}
