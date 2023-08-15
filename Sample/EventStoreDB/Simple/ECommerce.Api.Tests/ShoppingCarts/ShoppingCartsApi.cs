using ECommerce.Api.Requests;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts;

public static class ShoppingCartsApi
{
    public static RequestDefinition OpenShoppingCart(Guid? clientId = null) =>
        SEND(
            "Open ShoppingCart",
            POST,
            URI("/api/ShoppingCarts"),
            BODY(new OpenShoppingCartRequest(clientId ?? Guid.NewGuid()))
        );

    public static Guid OpenedShoppingCartId(this TestContext ctx) =>
        ctx.GetCreatedId<Guid>();
}
