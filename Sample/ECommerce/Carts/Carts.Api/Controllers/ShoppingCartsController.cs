using System;
using System.Threading.Tasks;
using Carts.Api.Requests;
using Carts.Api.Requests.Carts;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.GettingCartAtVersion;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.GettingCartHistory;
using Carts.ShoppingCarts.GettingCarts;
using Carts.ShoppingCarts.InitializingCart;
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
public class ShoppingCartsController: Controller
{
    private readonly ICommandBus commandBus;
    private readonly IQueryBus queryBus;
    private readonly IIdGenerator idGenerator;

    public ShoppingCartsController(
        ICommandBus commandBus,
        IQueryBus queryBus,
        IIdGenerator idGenerator)
    {
        this.commandBus = commandBus;
        this.queryBus = queryBus;
        this.idGenerator = idGenerator;
    }

    [HttpPost]
    public async Task<IActionResult> InitializeCart([FromBody] InitializeShoppingCartRequest? request)
    {
        var cartId = idGenerator.New();

        var command = InitializeShoppingCart.Create(
            cartId,
            request?.ClientId
        );

        await commandBus.Send(command);

        return Created("api/ShoppingCarts", cartId);
    }

    [HttpPost("{id}/products")]
    public async Task<IActionResult> AddProduct(
        Guid id,
        [FromBody] AddProductRequest? request
    )
    {
        var command = ShoppingCarts.AddingProduct.AddProduct.Create(
            id,
            ProductItem.Create(
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

    [HttpGet("{id}")]
    public async Task<ShoppingCartDetails> Get(Guid id)
    {
        var result = await queryBus.Send<GetCartById, ShoppingCartDetails>(GetCartById.Create(id));

        Response.TrySetETagResponseHeader(result.Version.ToString());

        return result;
    }

    [HttpGet]
    public async Task<PagedListResponse<ShoppingCartShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var pagedList = await queryBus.Send<GetCarts, IPagedList<ShoppingCartShortInfo>>(GetCarts.Create(pageNumber, pageSize));

        return pagedList.ToResponse();
    }


    [HttpGet("{id}/history")]
    public async Task<PagedListResponse<ShoppingCartHistory>> GetHistory(Guid id)
    {
        var pagedList = await queryBus.Send<GetCartHistory, IPagedList<ShoppingCartHistory>>(GetCartHistory.Create(id));

        return pagedList.ToResponse();
    }

    [HttpGet("{id}/versions")]
    public Task<ShoppingCartDetails> GetVersion(Guid id, [FromQuery] GetCartAtVersion? query)
    {
        return queryBus.Send<GetCartAtVersion, ShoppingCartDetails>(GetCartAtVersion.Create(id, query?.Version));
    }
}
