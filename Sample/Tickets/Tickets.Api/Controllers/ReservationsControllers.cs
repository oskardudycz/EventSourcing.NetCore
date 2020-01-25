using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Ids;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Requests;
using Tickets.Reservations.Commands;

namespace Tickets.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController: Controller
    {
        private readonly ICommandBus commandBus;

        private readonly IIdGenerator idGenerator;
        // private readonly IQueryBus _queryBus;

        public ReservationsController(
            ICommandBus commandBus,
            IIdGenerator idGenerator) //, IQueryBus queryBus)
        {
            Guard.Against.Null(commandBus, nameof(commandBus));
            Guard.Against.Null(idGenerator, nameof(idGenerator));
            this.commandBus = commandBus;
            this.idGenerator = idGenerator;
            //_queryBus = queryBus;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTentativeReservationRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var reservationId = idGenerator.New();

            var command = CreateTentativeReservation.Create(
                reservationId,
                request.SeatId
            );

            await commandBus.Send(command);

            return Created("api/Reservations", reservationId);
        }
    }
}
