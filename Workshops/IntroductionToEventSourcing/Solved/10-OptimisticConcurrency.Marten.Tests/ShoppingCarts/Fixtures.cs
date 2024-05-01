using Ogooreck.API;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using static Ogooreck.API.ApiSpecification;

namespace OptimisticConcurrency.Marten.Tests.ShoppingCarts;

public static class Fixtures
{
    public static string ClientsUrl(string apiPrefix, Guid clientId) => $"/api/{apiPrefix}/clients/{clientId}/";

    public static string ShoppingCartsUrl(string apiPrefix, Guid clientId) =>
        $"{ClientsUrl(apiPrefix, clientId)}shopping-carts/";

    public static string ShoppingCartUrl(string apiPrefix, Guid clientId, Guid shoppingCartId) =>
        $"{ShoppingCartsUrl(apiPrefix, clientId)}{shoppingCartId}/";

    public static string ConfirmShoppingCartUrl(string apiPrefix, Guid clientId, Guid shoppingCartId) =>
        $"{ShoppingCartUrl(apiPrefix, clientId, shoppingCartId)}confirm";

    public static string ShoppingCartProductItemsUrl(string apiPrefix, Guid clientId, Guid shoppingCartId) =>
        $"{ShoppingCartUrl(apiPrefix, clientId, shoppingCartId)}product-items/";

    public static string ShoppingCartProductItemUrl(string apiPrefix, Guid clientId, Guid shoppingCartId, Guid productId,
        int quantity = 1, decimal unitPrice = 100) =>
        $"{ShoppingCartProductItemsUrl(apiPrefix, clientId, shoppingCartId)}{productId}?unitPrice={unitPrice}&quantity={quantity}";
}

public static class Scenarios
{
    public static RequestDefinition OpenedShoppingCart(string apiPrefix, Guid clientId) =>
        SEND(POST, URI(Fixtures.ShoppingCartsUrl(apiPrefix, clientId)));

    public static RequestDefinition WithProductItem(string apiPrefix, Guid clientId, ProductItemRequest productItem, int ifMatch) =>
        SEND(
            POST,
            URI(ctx => Fixtures.ShoppingCartProductItemsUrl(apiPrefix, clientId, ctx.GetCreatedId<Guid>())),
            BODY(new AddProductRequest(productItem)),
            HEADERS(IF_MATCH(ifMatch))
        );

    public static RequestDefinition ThenConfirmed(string apiPrefix, Guid clientId, int ifMatch) =>
        SEND(
            POST,
            URI(ctx => Fixtures.ConfirmShoppingCartUrl(apiPrefix, clientId, ctx.GetCreatedId<Guid>())),
            HEADERS(IF_MATCH(ifMatch))
        );

    public static RequestDefinition ThenCanceled(string apiPrefix, Guid clientId, int ifMatch) =>
        SEND(
            DELETE,
            URI(ctx => Fixtures.ShoppingCartUrl(apiPrefix, clientId, ctx.GetCreatedId<Guid>())),
            HEADERS(IF_MATCH(ifMatch))
        );
}
