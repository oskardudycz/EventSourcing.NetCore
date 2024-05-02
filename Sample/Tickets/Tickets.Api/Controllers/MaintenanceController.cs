using Core.Commands;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Requests;
using Tickets.Maintenance.Commands;

namespace Tickets.Api.Controllers;

[Route("api/[controller]")]
public class MaintenanceController(ICommandBus commandBus): Controller
{
    [HttpPost("projections/rebuild")]
    public async Task<IActionResult> Rebuild([FromBody] RebuildProjectionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var command = RebuildProjection.Create(
            request.ProjectionName
        );

        await commandBus.Send(command);

        return Accepted();
    }
}
