using JasperFx.Core;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Reservations.RoomReservations.GettingRoomTypeAvailability;
using static Microsoft.AspNetCore.Http.Results;
using static Reservations.RoomReservations.GettingRoomTypeAvailability.GetRoomTypeAvailabilityForPeriod;

namespace Reservations.RoomReservations.ReservingRoom;

internal static class ReserveRoomEndpoint
{
    internal static IEndpointRouteBuilder UseReserveRoomEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/reservations/", async (
            [FromServices] IDocumentSession session,
            ReserveRoomRequest request,
            CancellationToken ct
        ) =>
        {
            var (roomType, from, to, guestId, numberOfPeople) = request;
            var reservationId = CombGuidIdGeneration.NewGuid().ToString();

            var dailyAvailability = await session.GetRoomTypeAvailabilityForPeriod(Of(roomType, from, to), ct);

            var command = ReserveRoom.FromApi(
                reservationId, roomType, from, to, guestId, numberOfPeople,
                DateTimeOffset.Now, dailyAvailability
            );

            session.Events.StartStream<RoomReservation>(reservationId, ReserveRoom.Handle(command));

            return Created($"/api/reservations/{reservationId}", reservationId);
        });

        return endpoints;
    }
}

public record ReserveRoomRequest(
    RoomType RoomType,
    DateOnly From,
    DateOnly To,
    string GuestId,
    int NumberOfPeople
);
