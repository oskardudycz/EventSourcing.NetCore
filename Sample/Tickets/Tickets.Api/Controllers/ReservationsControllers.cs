using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Requests;
using Tickets.Api.Responses;
using Tickets.Reservations.Commands;
using Tickets.Reservations.Projections;
using Tickets.Reservations.Queries;

namespace Tickets.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController: Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IQueryBus queryBus;

        private readonly IIdGenerator idGenerator;

        public ReservationsController(
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

        [HttpGet("{id}")]
        public Task<ReservationDetails> Get(Guid id)
        {
            return queryBus.Send<GetReservationById, ReservationDetails>(GetReservationById.Create(id));
        }

        [HttpGet]
        public async Task<PagedListResponse<ReservationShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var pagedList = await queryBus.Send<GetReservations, IPagedList<ReservationShortInfo>>(GetReservations.Create(pageNumber, pageSize));

            return PagedListResponse.From(pagedList);
        }


        [HttpGet("{id}/history")]
        public async Task<PagedListResponse<ReservationHistory>> GetHistory(Guid id)
        {
            var pagedList = await queryBus.Send<GetReservationHistory, IPagedList<ReservationHistory>>(GetReservationHistory.Create(id));

            return PagedListResponse.From(pagedList);
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
