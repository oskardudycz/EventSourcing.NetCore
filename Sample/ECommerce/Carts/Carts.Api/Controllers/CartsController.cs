using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Carts.Api.Requests.Carts;
using Carts.Carts.GettingCartAtVersion;
using Carts.Carts.GettingCartById;
using Carts.Carts.GettingCartHistory;
using Carts.Carts.GettingCarts;
using Carts.Carts.InitializingCart;
using Carts.Carts.Queries;
using Carts.Carts.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Marten.Responses;
using Core.Queries;
using Core.Responses;
using Marten.Pagination;

namespace Carts.Api.Controllers
{
    [Route("api/[controller]")]
    public class CartsController: Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IQueryBus queryBus;
        private readonly IIdGenerator idGenerator;

        public CartsController(
            ICommandBus commandBus,
            IQueryBus queryBus,
            IIdGenerator idGenerator)
        {
            Guard.Against.Null(commandBus, nameof(commandBus));
            Guard.Against.Null(queryBus, nameof(queryBus));
            Guard.Against.Null(idGenerator, nameof(idGenerator));

            this.commandBus = commandBus;
            this.queryBus = queryBus;
            this.idGenerator = idGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> InitCart([FromBody] InitCartRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var cartId = idGenerator.New();

            var command = InitializeCart.Create(
                cartId,
                request.ClientId
            );

            await commandBus.Send(command);

            return Created("api/Carts", cartId);
        }

        [HttpPost("{id}/products")]
        public async Task<IActionResult> AddProduct(Guid id, [FromBody] AddProductRequest request)
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.Null(request.ProductItem, nameof(request));

            var command = Carts.AddingProduct.AddProduct.Create(
                id,
                ProductItem.Create(
                    request.ProductItem.ProductId,
                    request.ProductItem.Quantity
                )
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpDelete("{id}/products")]
        public async Task<IActionResult> RemoveProduct(Guid id, [FromBody] RemoveProductRequest request)
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.Null(request.ProductItem, nameof(request));

            var command = Carts.RemovingProduct.RemoveProduct.Create(
                id,
                PricedProductItem.Create(
                    request.ProductItem.ProductId,
                    request.ProductItem.Quantity,
                    request.ProductItem.UnitPrice
                )
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpPut("{id}/confirmation")]
        public async Task<IActionResult> ConfirmCart(Guid id)
        {
            var command = Carts.ConfirmingCart.ConfirmCart.Create(
                id
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpGet("{id}")]
        public Task<CartDetails> Get(Guid id)
        {
            return queryBus.Send<GetCartById, CartDetails>(GetCartById.Create(id));
        }

        [HttpGet]
        public async Task<PagedListResponse<CartShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var pagedList = await queryBus.Send<GetCarts, IPagedList<CartShortInfo>>(GetCarts.Create(pageNumber, pageSize));

            return pagedList.ToResponse();
        }


        [HttpGet("{id}/history")]
        public async Task<PagedListResponse<CartHistory>> GetHistory(Guid id)
        {
            var pagedList = await queryBus.Send<GetCartHistory, IPagedList<CartHistory>>(GetCartHistory.Create(id));

            return pagedList.ToResponse();
        }

        [HttpGet("{id}/versions")]
        public Task<CartDetails> GetVersion(Guid id, [FromQuery] GetCartAtVersion query)
        {
            Guard.Against.Null(query, nameof(query));
            return queryBus.Send<GetCartAtVersion, CartDetails>(GetCartAtVersion.Create(id, query.Version));
        }
    }
}
