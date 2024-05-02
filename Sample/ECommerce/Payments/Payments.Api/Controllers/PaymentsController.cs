using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Payments.Api.Requests.Carts;
using Payments.Payments.TimingOutPayment;

namespace Payments.Api.Controllers;

[Route("api/[controller]")]
public class PaymentsController(
    ICommandBus commandBus,
    IQueryBus queryBus,
    IIdGenerator idGenerator)
    : Controller
{
    private readonly IQueryBus queryBus = queryBus;

    [HttpPost]
    public async Task<IActionResult> RequestPayment([FromBody] RequestPaymentRequest? request)
    {
        var paymentId = idGenerator.New();

        var command = Payments.RequestingPayment.RequestPayment.Create(
            paymentId,
            request?.OrderId,
            request?.Amount
        );

        await commandBus.Send(command);

        return Created($"/api/Payments/{paymentId}", paymentId);
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompletePayment(Guid id)
    {
        var command = Payments.CompletingPayment.CompletePayment.Create(
            id
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DiscardPayment(Guid id, [FromBody] DiscardPaymentRequest? request)
    {
        var command = Payments.DiscardingPayment.DiscardPayment.Create(
            id,
            request?.DiscardReason
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpPost("{id}/Timeout")]
    public async Task<IActionResult> TimeoutPayment(Guid id, [FromBody] TimeOutPaymentRequest? request)
    {
        var command = TimeOutPayment.Create(
            id,
            request?.TimedOutAt
        );

        await commandBus.Send(command);

        return Ok();
    }
}
