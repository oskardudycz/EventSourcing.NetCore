using System;

namespace Tickets.Api.Requests;

public class CreateTentativeReservationRequest
{
    public Guid SeatId { get; set; }
}