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

        // TODO in these samples in general, the semantics should be extended to be more representative of the real world
        // - you don't want to create orphan carts esp if they need to retry this API call
        // - you may want to allow users to shop before logging in, keeping the clientId in a cookie.
        //   After some time, you let them log in, but then you need to merge the content into their real cart
        // For now, the code remains in line with the C# version
        let cartId = CartId.generate();
        do! carts.Initialize(cartId, ClientId.parse request.clientId)
        return CreatedResult("api/ShoppingCarts", cartId) :> _
    }

    [<HttpPost("{id}/products")>]
    member _.AddProduct([<FromRoute>] id : Guid Nullable, [<FromBody>] request : AddProductRequest) : Async<IActionResult> = async {
        if obj.ReferenceEquals(null, request) then nameof request |> nullArg

        let CartId.Parse cartId, ProductId.Parse productId = id, request.productId
        do! carts.Add(cartId, productId, request.quantity)
        return OkResult() :> _
    }

    [<HttpDelete("{id}/products")>]
    member _.RemoveProduct([<FromRoute>] id : Guid Nullable, [<FromBody>] request : RemoveProductRequest) : Async<IActionResult> = async {
        if obj.ReferenceEquals(null, request) then nameof request |> nullArg

        let CartId.Parse cartId, ProductId.Parse productId = id, request.productId
        do! carts.Remove(cartId, productId, request.price)
        return OkResult() :> _
    }

    [<HttpDelete("{id}/confirmation")>]
    member _.ConfirmCart([<FromRoute>] id : Guid Nullable(*, [<FromBody>] request : ConfirmShoppingCartRequest*)) : Async<IActionResult> = async {
//        if obj.ReferenceEquals(null, request) then nameof request |> nullArg // TODO only relevant if we follow version-contingent style

        let (CartId.Parse cartId) = id
        do! carts.Confirm(cartId, DateTimeOffset.UtcNow)
        return OkResult() :> _
    }

    [<HttpGet("{id}")>]
    member _.Get([<FromRoute>] id : Guid Nullable) : Async<IActionResult> = async {
        let (CartId.Parse cartId) = id
        match! carts.Read cartId with
        | Some (res : ShoppingCart.Details.View) -> return OkObjectResult res :> _
        | None -> return NotFoundResult() :> _
    }

    (* TODO we dont produce a list like this atm - not porting for the moment as having an arbitrarily growing list like this does not really make
        sense; instead, we'll produce a cart summaries API
    [HttpGet]
    public Task<IReadOnlyList<ShoppingCartShortInfo>> Get(
        [FromServices] Func<GetCarts, CancellationToken, Task<IReadOnlyList<ShoppingCartShortInfo>>> query,
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    ) =>
        query(GetCarts.From(pageNumber, pageSize), ct);
*)
