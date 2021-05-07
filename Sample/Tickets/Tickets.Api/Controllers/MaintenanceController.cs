using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Requests;
using Tickets.Maintenance.Commands;

namespace Tickets.Api.Controllers
{
    [Route("api/[controller]")]
    public class MaintenanceController: Controller
    {
        private readonly ICommandBus commandBus;

        public MaintenanceController(
            ICommandBus commandBus)
        {
            Guard.Against.Null(commandBus, nameof(commandBus));

            this.commandBus = commandBus;
        }

        [HttpPost("projections/rebuild")]
        public async Task<IActionResult> Rebuild([FromBody] RebuildProjectionRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var command = RebuildProjection.Create(
                request.ProjectionName
            );

            await commandBus.Send(command);

            return Accepted();
        }
    }
}
