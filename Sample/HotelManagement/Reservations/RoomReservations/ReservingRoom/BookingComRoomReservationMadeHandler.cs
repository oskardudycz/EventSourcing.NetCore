using Core.Events;
using JasperFx.Core;
using Marten;
using Reservations.Guests.GettingGuestByExternalId;
using static Reservations.Guests.GuestExternalId;
using static Reservations.Guests.GettingGuestByExternalId.GetGuestIdByExternalId;

namespace Reservations.RoomReservations.ReservingRoom;

public record BookingComRoomReservationMade
(
    string ReservationId,
    string RoomType,
    DateOnly Start,
    DateOnly End,
    string GuestProfileId,
    int GuestsCounts,
    DateTimeOffset MadeAt
);

public class BookingComRoomReservationMadeHandler: IEventHandler<BookingComRoomReservationMade>
{
    private readonly IDocumentSession session;

    public BookingComRoomReservationMadeHandler(IDocumentSession session) =>
        this.session = session;

    public async Task Handle(BookingComRoomReservationMade @event, CancellationToken ct)
    {
        var (bookingComReservationId, roomTypeText, from, to, bookingComGuestId, numberOfPeople, madeAt) = @event;
        var reservationId = CombGuidIdGeneration.NewGuid().ToString();

        var guestId = await Query(new GetGuestIdByExternalId(FromPrefix("BCOM", bookingComGuestId)), ct);
        var roomType = Enum.Parse<RoomType>(roomTypeText);

        var command = ReserveRoom.FromExternal(
            reservationId, bookingComReservationId, roomType, from, to, guestId.Value, numberOfPeople, madeAt
        );

        session.Events.StartStream<RoomReservation>(reservationId, ReserveRoom.Handle(command));

        await session.SaveChangesAsync(ct);
    }
}
