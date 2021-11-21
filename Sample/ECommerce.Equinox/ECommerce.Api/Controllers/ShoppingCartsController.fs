namespace ECommerce.Api.Controllers

open ECommerce.Domain
open Microsoft.AspNetCore.Mvc
open System

type InitializeShoppingCartRequest = { clientId : Guid Nullable }
type AddProductRequest = { productId : Guid; quantity : int }
type RemoveProductRequest = { productId : Guid; price : decimal }

[<ApiController>]
[<Route("api/[controller]")>]
type ShoppingCartsController(carts : ShoppingCart.Service) =
    inherit ControllerBase()

    [<HttpPost>]
    member _.InitializeCart([<FromBody>] request : InitializeShoppingCartRequest) : Async<IActionResult> = async {
        if obj.ReferenceEquals(null, request) then nameof request |> nullArg

        // TODO this should be resolved via an idempotent lookup in the Client's list
        let cartId = CartId.generate();
        do! carts.Initialize(cartId, ClientId.parse request.clientId)
        return CreatedResult("api/ShoppingCarts", cartId);
    }

    [<HttpPost("{id}/products")>]
    member _.AddProduct([<FromRoute>] id : Guid Nullable, [<FromBody>] request : AddProductRequest) : Async<IActionResult> = async {
        if obj.ReferenceEquals(null, request) then nameof request |> nullArg

        // TODO C# read model has a cartId
        let CartId.Parse cartId, ProductId.Parse productId = id, request.productId
        do! carts.Add(cartId, productId, request.quantity)
        return OkResult();
    }

    // TODO remove shoppingCartId from request in C#
    [<HttpDelete("{id}/products")>]
    member _.RemoveProduct([<FromRoute>] id : Guid Nullable, [<FromBody>] request : RemoveProductRequest) : Async<IActionResult> = async {
        if obj.ReferenceEquals(null, request) then nameof request |> nullArg

        let CartId.Parse cartId, ProductId.Parse productId = id, request.productId
        do! carts.Remove(cartId, productId, request.price)
        return OkResult();
    }

    // TODO remove shoppingCartId from request in C#
    [<HttpDelete("{id}/confirmation")>]
    member _.ConfirmCart([<FromRoute>] id : Guid Nullable(*, [<FromBody>] request : ConfirmShoppingCartRequest*)) : Async<IActionResult> = async {
//        if obj.ReferenceEquals(null, request) then nameof request |> nullArg

        let (CartId.Parse cartId) = id
        do! carts.Confirm(cartId, DateTime.UtcNow)
        return OkResult();
    }

    [<HttpGet("{id}")>]
    member _.Get([<FromRoute>] id : Guid Nullable) : Async<IActionResult> = async {

        let (CartId.Parse cartId) = id
        match! carts.Read cartId with
        | Some (res : ShoppingCart.Details.View) -> return OkObjectResult res
        | None -> return NotFoundResult()
    }

(* TODO
    [HttpGet]
    public Task<IReadOnlyList<ShoppingCartShortInfo>> Get(
        [FromServices] Func<GetCarts, CancellationToken, Task<IReadOnlyList<ShoppingCartShortInfo>>> query,
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    ) =>
        query(GetCarts.From(pageNumber, pageSize), ct);
*)
