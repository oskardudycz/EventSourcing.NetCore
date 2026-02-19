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
            BODY(new OpenShoppingCartRequest(clientId ?? Guid.CreateVersion7()))
        );

    public static Guid OpenedShoppingCartId(this TestContext ctx) =>
        ctx.GetCreatedId<Guid>();
}
