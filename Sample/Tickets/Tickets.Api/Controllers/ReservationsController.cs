using Core.Commands;
using Core.Ids;
using Core.Queries;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Requests;
using Tickets.Api.Responses;
using Tickets.Reservations.CancellingReservation;
using Tickets.Reservations.ChangingReservationSeat;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Reservations.CreatingTentativeReservation;
using Tickets.Reservations.GettingReservationAtVersion;
using Tickets.Reservations.GettingReservationById;
using Tickets.Reservations.GettingReservationHistory;
using Tickets.Reservations.GettingReservations;

namespace Tickets.Api.Controllers;

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
        this.commandBus = commandBus;
        this.queryBus = queryBus;
        this.idGenerator = idGenerator;
    }

    [HttpGet("{id}")]
    public Task<ReservationDetails> Get(Guid id)
    {
        return queryBus.Send<GetReservationById, ReservationDetails>(new GetReservationById(id));
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

    [HttpGet("{id}/versions")]
    public Task<ReservationDetails> GetVersion(Guid id, [FromQuery] GetReservationDetailsAtVersion request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return queryBus.Send<GetReservationAtVersion, ReservationDetails>(GetReservationAtVersion.Create(id, request.Version));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTentative([FromBody] CreateTentativeReservationRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var reservationId = idGenerator.New();

        var command = CreateTentativeReservation.Create(
            reservationId,
            request.SeatId
        );

        await commandBus.Send(command);

        return Created("api/Reservations", reservationId);
    }


    [HttpPost("{id}/seat")]
    public async Task<IActionResult> ChangeSeat(Guid id, [FromBody] ChangeSeatRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var command = ChangeReservationSeat.Create(
            id,
            request.SeatId
        );

        await commandBus.Send(command);

        return Ok();
    }

    [HttpPut("{id}/confirmation")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var command = ConfirmReservation.Create(
            id
        );

        await commandBus.Send(command);

        return Ok();
    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var command = CancelReservation.Create(
            id
        );

        await commandBus.Send(command);

        return Ok();
    }
}