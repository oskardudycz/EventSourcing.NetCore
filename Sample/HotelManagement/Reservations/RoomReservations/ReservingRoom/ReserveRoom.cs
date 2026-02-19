using Core.Validation;
using Reservations.RoomReservations.GettingRoomTypeAvailability;

namespace Reservations.RoomReservations.ReservingRoom;

public record ReserveRoom
(
    string ReservationId,
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
            command.ReservationId,
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
            id.NotEmpty(),
            roomType.NotEmpty(),
            from.NotEmpty(),
            to.NotEmpty().GreaterOrEqualThan(from),
            guestId.NotEmpty(),
            numberOfPeople.NotEmpty(),
            now.NotEmpty(),
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
            id.NotEmpty(),
            roomType.NotEmpty(),
            from.NotEmpty(),
            to.NotEmpty().GreaterOrEqualThan(from),
            guestId.NotEmpty(),
            numberOfPeople.NotEmpty(),
            now.NotEmpty(),
            ReservationSource.External,
            [],
            externalId.NotEmpty()
        );
}
