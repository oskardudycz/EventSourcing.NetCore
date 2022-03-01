using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.WebApi.Headers;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts.AddingProductItem;
using ECommerce.ShoppingCarts.Confirming;
using ECommerce.ShoppingCarts.GettingCartById;
using ECommerce.ShoppingCarts.GettingCarts;
using ECommerce.ShoppingCarts.Initializing;
using ECommerce.ShoppingCarts.ProductItems;
using ECommerce.ShoppingCarts.RemovingProductItem;

namespace ECommerce.Api.Controllers;

[Route("api/[controller]")]
public class ShoppingCartsController: Controller
{
    [HttpPost]
    public async Task<IActionResult> InitializeCart(
        [FromServices] Func<Guid> generateId,
        [FromServices] Func<InitializeShoppingCart, CancellationToken, ValueTask> handle,
        [FromBody] InitializeShoppingCartRequest? request,
        CancellationToken ct
    )
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var cartId = generateId();

        var command = InitializeShoppingCart.From(
            cartId,
            request.ClientId
        );

        await handle(command, ct);

        return Created("api/ShoppingCarts", cartId);
    }

    [HttpPost("{id}/products")]
    public async Task<IActionResult> AddProduct(
        [FromServices] Func<AddProductItemToShoppingCart, CancellationToken, ValueTask> handle,
        [FromRoute] Guid id,
        [FromBody] AddProductRequest? request,
        CancellationToken ct
    )
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var command = AddProductItemToShoppingCart.From(
            id,
            ProductItem.From(
                request.ProductItem?.ProductId,
                request.ProductItem?.Quantity
            )
        );

        await handle(command, ct);

        return Ok();
    }

    [HttpDelete("{id}/products/{productId}")]
    public async Task<IActionResult> RemoveProduct(
        [FromServices] Func<RemoveProductItemFromShoppingCart, CancellationToken, ValueTask> handle,
        Guid id,
        [FromRoute]Guid? productId,
        [FromQuery]int? quantity,
        [FromQuery]decimal? unitPrice,
        CancellationToken ct
    )
    {
        var command = RemoveProductItemFromShoppingCart.From(
            id,
            PricedProductItem.From(
                ProductItem.From(
                    productId,
                    quantity
                ),
                unitPrice
            )
        );

        await handle(command, ct);

        return NoContent();
    }

    [HttpPut("{id}/confirmation")]
    public async Task<IActionResult> ConfirmCart(
        [FromServices] Func<ConfirmShoppingCart, CancellationToken, ValueTask> handle,
        Guid id,
        [FromBody] ConfirmShoppingCartRequest request,
        CancellationToken ct
    )
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var command =
            ConfirmShoppingCart.From(id);

        await handle(command, ct);

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(
        [FromServices] Func<GetCartById, CancellationToken, Task<ShoppingCartDetails?>> query,
        Guid id,
        CancellationToken ct
    )
    {
        var result = await query(GetCartById.From(id), ct);

        if (result == null)
            return NotFound();

        Response.TrySetETagResponseHeader(result.Version.ToString());
        return Ok(result);
    }

    [HttpGet]
    public Task<IReadOnlyList<ShoppingCartShortInfo>> Get(
        [FromServices] Func<GetCarts, CancellationToken, Task<IReadOnlyList<ShoppingCartShortInfo>>> query,
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    ) =>
        query(GetCarts.From(pageNumber, pageSize), ct);
}
