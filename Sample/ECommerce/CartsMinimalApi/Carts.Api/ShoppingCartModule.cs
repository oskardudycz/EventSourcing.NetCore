using Carter;
using Carter.Request;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.CancellingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.RemovingProduct;

public class ShoppingCartModule: ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/shoppingcart", OpenCart);
        app.MapPost("/shoppingcart/{cartId:guid}/products", AddProduct);
        app.MapDelete("/shoppingcart/{cartId:guid}/products/{productId:guid}", RemoveProduct);
        app.MapPut("/shoppingcart/{cartId:guid}/confirmation", ConfirmCart);
        app.MapDelete("/shoppingcart/{cartId:guid}", CancelCart);
    }

    private IResult CancelCart(HttpContext context, Guid cartId, ICancelCartService cancelCartService)
    {
        cancelCartService.CancelCart(cartId);
        return Results.StatusCode(204);
    }

    private IResult ConfirmCart(HttpContext context, Guid cartId, IConfirmCartService confirmCartService)
    {
        confirmCartService.Confirm(cartId);
        return Results.StatusCode(204);
    }

    private IResult RemoveProduct(HttpContext context, Guid cartId, Guid productId,
        IRemoveProductService removeProductService)
    {
        removeProductService.RemoveProduct(cartId, productId, context.Request.Query.As<int?>("quantity"),
            context.Request.Query.As<decimal?>("unitPrice"));
        return Results.StatusCode(204);
    }

    private IResult AddProduct(HttpContext context, Guid cartId, AddProductRequest model,
        IAddProductService addProductService)
    {
        addProductService.AddProduct(cartId, model);
        return Results.StatusCode(204);
    }

    private IResult OpenCart(HttpContext context, OpenShoppingCartRequest model, IOpenCartService openCartService)
    {
        var cartId = openCartService.OpenCart(model);

        return Results.Created($"/shoppingcart/{cartId}", null);
    }
}
