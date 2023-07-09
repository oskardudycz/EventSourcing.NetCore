using Core.Validation;
using Reservations.RoomReservations.GettingRoomTypeAvailability;

namespace Reservations.RoomReservations.ReservingRoom;

public record ReserveRoom
(
    string Id,
    RoomType RoomType,
    DateOnly From,
    DateOnly To,
    string GuestId,
    int NumberOfPeople,
    DateTimeOffset Now,
    ReservationSource ReservationSource,
    IReadOnlyList<DailyRoomTypeAvailability> DailyAvailability,
    string? ExternalId
)
{
    public static RoomReserved Handle(ReserveRoom command)
    {
        var reservationSource = command.ReservationSource;

        var dailyAvailability = command.DailyAvailability;

        if (reservationSource == ReservationSource.Api && dailyAvailability.Any(a => a.AvailableRooms < 1))
            throw new InvalidOperationException("Not enough available rooms!");

        return new RoomReserved(
            command.Id,
            command.ExternalId,
            command.RoomType,
            command.From,
            command.To,
            command.GuestId,
            command.NumberOfPeople,
            command.ReservationSource,
            command.Now
        );
    }

    public static ReserveRoom FromApi(
        string id,
        RoomType roomType,
        DateOnly from,
        DateOnly to,
        string guestId,
        int numberOfPeople,
        DateTimeOffset now,
        IReadOnlyList<DailyRoomTypeAvailability> dailyAvailability
    ) =>
        new(
            id.AssertNotEmpty(),
            roomType.AssertNotEmpty(),
            from.AssertNotEmpty(),
            to.AssertNotEmpty().AssertGreaterOrEqualThan(from),
            guestId.AssertNotEmpty(),
            numberOfPeople.AssertNotEmpty(),
            now.AssertNotEmpty(),
            ReservationSource.Api,
            dailyAvailability,
            null
        );

    public static ReserveRoom FromExternal(
        string id,
        string externalId,
        RoomType roomType,
        DateOnly from,
        DateOnly to,
        string guestId,
        int numberOfPeople,
        DateTimeOffset now
    ) =>
        new(
            id.AssertNotEmpty(),
            roomType.AssertNotEmpty(),
            from.AssertNotEmpty(),
            to.AssertNotEmpty().AssertGreaterOrEqualThan(from),
            guestId.AssertNotEmpty(),
            numberOfPeople.AssertNotEmpty(),
            now.AssertNotEmpty(),
            ReservationSource.External,
            Array.Empty<DailyRoomTypeAvailability>(),
            externalId.AssertNotEmpty()
        );
}
