using Carts.Api.Requests;
using Carts.ShoppingCarts.CancelingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.GettingCartAtVersion;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.GettingCartHistory;
using Carts.ShoppingCarts.GettingCarts;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.Products;
using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Marten.Responses;
using Core.Queries;
using Core.Responses;
using Core.WebApi.Headers;
using Marten.Pagination;

namespace Carts.Api.Controllers;

[Route("api/[controller]")]
public class ShoppingCartsController(
    ICommandBus commandBus,
    IQueryBus queryBus,
    IIdGenerator idGenerator)
    : Controller
{
    [HttpPost]
    public async Task<IActionResult> OpenCart([FromBody] OpenShoppingCartRequest? request)
    {
        var cartId = idGenerator.New();

        var command = OpenShoppingCart.Create(
            cartId,
            request?.ClientId
        );

        await commandBus.Send(command);

        return Created($"/api/ShoppingCarts/{cartId}", cartId);
    }

    [HttpPost("{id}/products")]
    public async Task<IActionResult> AddProduct(
        Guid id,
        [FromBody] AddProductRequest? request
    )
    {
        var command = ShoppingCarts.AddingProduct.AddProduct.Create(
            id,
            ProductItem.From(
                request?.ProductItem?.ProductId,
                request?.ProductItem?.Quantity
            )
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpDelete("{id}/products/{productId}")]
    public async Task<IActionResult> RemoveProduct(
        Guid id,
        [FromRoute]Guid? productId,
        [FromQuery]int? quantity,
        [FromQuery]decimal? unitPrice
    )
    {
        var command = ShoppingCarts.RemovingProduct.RemoveProduct.Create(
            id,
            PricedProductItem.Create(
                productId,
                quantity,
                unitPrice
            )
        );

        await commandBus.Send(command);

        return NoContent();
    }

    [HttpPut("{id}/confirmation")]
    public async Task<IActionResult> ConfirmCart(Guid id)
    {
        var command = ConfirmShoppingCart.Create(
            id
        );

        await commandBus.Send(command);

        return Ok();
    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelCart(Guid id)
    {
        var command = CancelShoppingCart.Create(
            id
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<ShoppingCartDetails> Get(Guid id)
    {
        var result = await queryBus.Query<GetCartById, ShoppingCartDetails>(GetCartById.For(id));

        Response.TrySetETagResponseHeader(result.Version);

        return result;
    }

    [HttpGet]
    public async Task<PagedListResponse<ShoppingCartShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var pagedList = await queryBus.Query<GetCarts, IPagedList<ShoppingCartShortInfo>>(GetCarts.Create(pageNumber, pageSize));

        return pagedList.ToResponse();
    }

    [HttpGet("{id}/history")]
    public async Task<PagedListResponse<ShoppingCartHistory>> GetHistory(Guid id)
    {
        var pagedList = await queryBus.Query<GetCartHistory, IPagedList<ShoppingCartHistory>>(GetCartHistory.Create(id));

        return pagedList.ToResponse();
    }

    [HttpGet("{id}/versions")]
    public Task<ShoppingCartDetails> GetVersion(Guid id, [FromQuery] GetCartAtVersionRequest? query)
    {
        return queryBus.Query<GetCartAtVersion, ShoppingCartDetails>(GetCartAtVersion.Create(id, query?.Version));
    }
}
