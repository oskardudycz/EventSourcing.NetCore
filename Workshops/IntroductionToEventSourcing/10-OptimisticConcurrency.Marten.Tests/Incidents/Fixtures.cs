using ApplicationLogic.Marten.Immutable.ShoppingCarts;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace ApplicationLogic.Marten.Tests.Incidents;

public static class Fixtures
{
    public static string Clients(string apiPrefix, Guid clientId) => $"/api/{apiPrefix}/clients/{clientId}/";

    public static string ShoppingCarts(string apiPrefix, Guid clientId) =>
        $"{Clients(apiPrefix, clientId)}shopping-carts/";

    public static string ShoppingCart(string apiPrefix, Guid clientId, Guid shoppingCartId) =>
        $"{ShoppingCarts(apiPrefix, clientId)}{shoppingCartId}/";

    public static string ConfirmShoppingCart(string apiPrefix, Guid clientId, Guid shoppingCartId) =>
        $"{ShoppingCart(apiPrefix, clientId, shoppingCartId)}confirm";

    public static string ShoppingCartProductItems(string apiPrefix, Guid clientId, Guid shoppingCartId) =>
        $"{ShoppingCart(apiPrefix, clientId, shoppingCartId)}product-items/";

    public static string ShoppingCartProductItem(string apiPrefix, Guid clientId, Guid shoppingCartId, Guid productId,
        int quantity = 1, decimal unitPrice = 100) =>
        $"{ShoppingCartProductItems(apiPrefix, clientId, shoppingCartId)}{productId}?unitPrice={unitPrice}&quantity={quantity}";
}

public static class Scenarios
{
    public static RequestDefinition OpenedShoppingCart(string apiPrefix, Guid clientId) =>
        SEND(POST, URI(Fixtures.ShoppingCarts(apiPrefix, clientId)));

    public static RequestDefinition WithProductItem(string apiPrefix, Guid clientId, ProductItemRequest productItem) =>
        SEND(
            POST,
            URI(ctx => Fixtures.ShoppingCartProductItems(apiPrefix, clientId, ctx.GetCreatedId<Guid>())),
            BODY(new AddProductRequest(productItem))
        );

    public static RequestDefinition ThenConfirmed(string apiPrefix, Guid clientId) =>
        SEND(
            POST,
            URI(ctx => Fixtures.ConfirmShoppingCart(apiPrefix, clientId, ctx.GetCreatedId<Guid>()))
        );

    public static RequestDefinition ThenCanceled(string apiPrefix, Guid clientId) =>
        SEND(
            DELETE,
            URI(ctx => Fixtures.ShoppingCart(apiPrefix, clientId, ctx.GetCreatedId<Guid>()))
        );
}
