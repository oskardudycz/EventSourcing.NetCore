using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Carts.Api.Requests.Carts;
using Carts.Carts.Commands;
using Carts.Carts.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Commands = Carts.Carts.Commands;

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

            var command = Commands.InitCart.Create(
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

            var command = Commands.AddProduct.Create(
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

            var command = Commands.RemoveProduct.Create(
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
            var command = Commands.ConfirmCart.Create(
                id
            );

            await commandBus.Send(command);

            return Ok();
        }
    }
}
