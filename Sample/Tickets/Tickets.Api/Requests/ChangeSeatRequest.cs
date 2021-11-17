using System;

namespace Tickets.Api.Requests;

public class ChangeSeatRequest
{
    public Guid SeatId { get; set; }
}