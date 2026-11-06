using Marten.Events.Projections;

namespace Reservations.RoomReservations.GettingRoomTypeAvailability;

public record DailyRoomTypeAvailability
(
    string Id,
    DateOnly Date,
    RoomType RoomType,
    int ReservedRooms,
    int Capacity,
    int AllowedOverbooking
)
{
    public int CapacityWithOverbooking => Capacity + AllowedOverbooking;

    public int AvailableRooms => CapacityWithOverbooking - ReservedRooms;

    public int Overbooked => ReservedRooms - Capacity;
    
    public int OverbookedOverTheLimit => ReservedRooms - CapacityWithOverbooking;
}

public class DailyRoomTypeAvailabilityProjection: MultiStreamProjection<DailyRoomTypeAvailability, string>
{
    public DailyRoomTypeAvailabilityProjection() =>
        Identities<RoomReserved>(e =>
            Enumerable.Range(0, e.To.DayNumber - e.From.DayNumber)
                .Select(offset => $"{e.RoomType}_{e.To.AddDays(offset)}")
                .ToList()
        );

    public DailyRoomTypeAvailability Apply(DailyRoomTypeAvailability availability, RoomReserved reserved) =>
        availability with { ReservedRooms = availability.ReservedRooms + 1 };
}
