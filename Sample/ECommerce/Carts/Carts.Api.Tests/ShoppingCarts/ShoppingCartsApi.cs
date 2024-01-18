using Carts.Api.Requests;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts;

public static class ShoppingCartsApi
{
    public static RequestDefinition OpenShoppingCart(Guid? clientId = null) =>
        SEND(
            "Open ShoppingCart",
            POST,
            URI("/api/ShoppingCarts"),
            BODY(new OpenShoppingCartRequest(clientId ?? Guid.NewGuid()))
        );


    public static RequestDefinition AddProductItem(ProductItemRequest productItem, int ifMatch = 1) =>
        SEND(
            "Add new product",
            POST,
            URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/products"),
            BODY(new AddProductRequest(productItem)),
            HEADERS(IF_MATCH(ifMatch))
        );

    public static Guid OpenedShoppingCartId(this TestContext ctx) =>
        ctx.GetCreatedId<Guid>();
}
