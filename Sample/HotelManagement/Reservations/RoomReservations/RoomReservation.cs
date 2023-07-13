namespace Reservations.RoomReservations;

public record RoomReserved
(
    string ReservationId,
    string? ExternalReservationId,
    RoomType RoomType,
    DateOnly From,
    DateOnly To,
    string GuestId,
    int NumberOfPeople,
    ReservationSource Source,
    DateTimeOffset MadeAt
);

public record RoomReservationConfirmed
(
    string ReservationId,
    DateTimeOffset ConfirmedAt
);

public record RoomReservationCancelled
(
    string ReservationId,
    DateTimeOffset CancelledAt
);

public record RoomReservation
(
    string Id,
    RoomType RoomType,
    DateOnly From,
    DateOnly To,
    string GuestId,
    int NumberOfPeople,
    ReservationSource Source,
    ReservationStatus Status,
    DateTimeOffset MadeAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CancelledAt
)
{
    public static RoomReservation Create(RoomReserved reserved) =>
        new(
            reserved.ReservationId,
            reserved.RoomType,
            reserved.From,
            reserved.To,
            reserved.GuestId,
            reserved.NumberOfPeople,
            reserved.Source,
            reserved.Source == ReservationSource.External ? ReservationStatus.Confirmed : ReservationStatus.Pending,
            reserved.MadeAt,
            reserved.Source == ReservationSource.External ? reserved.MadeAt : null,
            null
        );

    public RoomReservation Apply(RoomReservationConfirmed confirmed) =>
        this with
        {
            Status = ReservationStatus.Confirmed,
            ConfirmedAt = confirmed.ConfirmedAt
        };

    public RoomReservation Apply(RoomReservationCancelled confirmed) =>
        this with
        {
            Status = ReservationStatus.Cancelled,
            ConfirmedAt = confirmed.CancelledAt
        };

}

public enum RoomType
{
    Single = 1,
    Twin = 2,
    King = 3
}

public enum ReservationSource
{
    Api,
    External
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled
}
