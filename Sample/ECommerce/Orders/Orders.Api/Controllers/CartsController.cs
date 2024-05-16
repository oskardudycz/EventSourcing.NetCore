using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Orders.Api.Requests.Carts;
using Orders.Orders.CompletingOrder;
using Orders.Orders.GettingOrderStatus;
using Orders.Orders.InitializingOrder;
using Orders.Products;

namespace Orders.Api.Controllers;

[Route("api/[controller]")]
public class OrdersController(
    ICommandBus commandBus,
    IQueryBus queryBus,
    IIdGenerator idGenerator)
    : Controller
{
    private readonly IQueryBus queryBus = queryBus;

    [HttpPost]
    public async Task<IActionResult> InitOrder([FromBody] InitOrderRequest? request)
    {
        var orderId = idGenerator.New();

        var command = InitializeOrder.Create(
            orderId,
            request?.ClientId,
            request?.ProductItems?.Select(
                pi => PricedProductItem.Create(pi.ProductId, pi.Quantity,pi.UnitPrice)).ToList(),
            request?.TotalPrice
        );

        await commandBus.Send(command);

        return Created($"/api/Orders/{orderId}", orderId);
    }

    [HttpPost("{id}/products")]
    public async Task<IActionResult> RecordOrderPayment(Guid id, [FromBody] RecordOrderPaymentRequest? request)
    {
        var command = Orders.RecordingOrderPayment.RecordOrderPayment.Create(
            id,
            request?.PaymentId,
            request?.PaymentRecordedAt
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest? request)
    {
        var command = Orders.CancellingOrder.CancelOrder.Create(
            id,
            request?.CancellationReason
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpPut("{id}/confirmation")]
    public async Task<IActionResult> ConfirmOrder(Guid id)
    {
        var command = CompleteOrder.For(
            id
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var query = GetOrderStatus.For(id);

        return await queryBus.Query<GetOrderStatus, OrderDetails?>(query) is {} details ?
                Ok(details) : NotFound();
    }
}
