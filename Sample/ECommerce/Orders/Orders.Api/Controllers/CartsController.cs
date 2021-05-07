using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Orders.Api.Requests.Carts;
using Orders.Products.ValueObjects;
using Commands = Orders.Orders.Commands;

namespace Orders.Api.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController: Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IQueryBus queryBus;
        private readonly IIdGenerator idGenerator;

        public OrdersController(
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
        public async Task<IActionResult> InitOrder([FromBody] InitOrderRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var orderId = idGenerator.New();

            var command = Commands.InitOrder.Create(
                orderId,
                request.ClientId,
                request.ProductItems?.Select(
                    pi => PricedProductItem.Create(pi.ProductId, pi.Quantity,pi.UnitPrice)).ToList(),
                request.TotalPrice
            );

            await commandBus.Send(command);

            return Created("api/Orders", orderId);
        }

        [HttpPost("{id}/products")]
        public async Task<IActionResult> RecordOrderPayment(Guid id, [FromBody] RecordOrderPaymentRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var command = Commands.RecordOrderPayment.Create(
                id,
                request.PaymentId,
                request.PaymentRecordedAt
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var command = Commands.CancelOrder.Create(
                id,
                request.CancellationReason
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpPut("{id}/confirmation")]
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            var command = Commands.CompleteOrder.Create(
                id
            );

            await commandBus.Send(command);

            return Ok();
        }
    }
}
