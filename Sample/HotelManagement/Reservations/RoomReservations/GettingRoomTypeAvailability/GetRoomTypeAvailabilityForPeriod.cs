using Core.Validation;
using Marten;

namespace Reservations.RoomReservations.GettingRoomTypeAvailability;

public record GetRoomTypeAvailabilityForPeriod(
    RoomType RoomType,
    DateOnly From,
    DateOnly To
)
{
    public static GetRoomTypeAvailabilityForPeriod Of(
        RoomType roomType,
        DateOnly from,
        DateOnly to
    ) =>
        new(
            roomType.NotEmpty(),
            from.NotEmpty(),
            to.NotEmpty().GreaterOrEqualThan(from)
        );
}

public static class GetRoomTypeAvailabilityForPeriodHandler
{
    public static Task<IReadOnlyList<DailyRoomTypeAvailability>> GetRoomTypeAvailabilityForPeriod(
        this IQuerySession session,
        GetRoomTypeAvailabilityForPeriod query,
        CancellationToken ct
    ) =>
        session.Query<DailyRoomTypeAvailability>()
            .Where(day => day.RoomType == query.RoomType && day.Date >= query.From && day.Date <= query.To)
            .ToListAsync(token: ct);
}
