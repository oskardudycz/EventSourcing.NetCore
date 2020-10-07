using System;

namespace Shipments.Api.Requests
{
    public class CreateTentativeReservationRequest
    {
        public Guid SeatId { get; set; }
    }
}
