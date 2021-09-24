using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts.Initializing;

namespace ECommerce.Api.Controllers
{
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

        // [HttpPost("{id}/products")]
        // public async Task<IActionResult> AddProduct(
        //     [FromRoute] Guid id, [FromBody] AddProductRequest? request)
        // {
        //     var command = Carts.AddingProduct.AddProduct.Create(
        //         id,
        //         ProductItem.Create(
        //             request?.ProductItem?.ProductId,
        //             request?.ProductItem?.Quantity
        //         )
        //     );
        //
        //     await commandBus.Send(command);
        //
        //     return Ok();
        // }
        //
        // [HttpDelete("{id}/products")]
        // public async Task<IActionResult> RemoveProduct(Guid id, [FromBody] RemoveProductRequest request)
        // {
        //     var command = Carts.RemovingProduct.RemoveProduct.Create(
        //         id,
        //         PricedProductItem.Create(
        //             request?.ProductItem?.ProductId,
        //             request?.ProductItem?.Quantity,
        //             request?.ProductItem?.UnitPrice
        //         )
        //     );
        //
        //     await commandBus.Send(command);
        //
        //     return Ok();
        // }
        //
        // [HttpPut("{id}/confirmation")]
        // public async Task<IActionResult> ConfirmCart(Guid id)
        // {
        //     var command = Carts.ConfirmingCart.ConfirmCart.Create(
        //         id
        //     );
        //
        //     await commandBus.Send(command);
        //
        //     return Ok();
        // }
        //
        // [HttpGet("{id}")]
        // public Task<CartDetails> Get(Guid id)
        // {
        //     return queryBus.Send<GetCartById, CartDetails>(GetCartById.Create(id));
        // }
        //
        // [HttpGet]
        // public async Task<PagedListResponse<CartShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        // {
        //     var pagedList = await queryBus.Send<GetCarts, IPagedList<CartShortInfo>>(GetCarts.Create(pageNumber, pageSize));
        //
        //     return pagedList.ToResponse();
        // }
        //
        //
        // [HttpGet("{id}/history")]
        // public async Task<PagedListResponse<CartHistory>> GetHistory(Guid id)
        // {
        //     var pagedList = await queryBus.Send<GetCartHistory, IPagedList<CartHistory>>(GetCartHistory.Create(id));
        //
        //     return pagedList.ToResponse();
        // }
        //
        // [HttpGet("{id}/versions")]
        // public Task<CartDetails> GetVersion(Guid id, [FromQuery] GetCartAtVersion? query)
        // {
        //     return queryBus.Send<GetCartAtVersion, CartDetails>(GetCartAtVersion.Create(id, query?.Version));
        // }
    }
}
