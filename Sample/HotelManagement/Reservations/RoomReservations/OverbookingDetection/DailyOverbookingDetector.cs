using Core.Events;
using Marten;
using Marten.Services;
using Reservations.RoomReservations.GettingRoomTypeAvailability;

namespace Reservations.RoomReservations.OverbookingDetection;

public record DailyOverbookingDetected
(
    RoomType RoomType,
    DateOnly Date,
    int OverBookedCount,
    int OverBookedOverTheLimitCount
);

public class DailyOverbookingDetector: IChangeListener
{
    private readonly IEventBus eventBus;

    public DailyOverbookingDetector(IEventBus eventBus) =>
        this.eventBus = eventBus;

    public async Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
    {
        var events = commit.Inserted.OfType<DailyRoomTypeAvailability>()
            .Union(commit.Updated.OfType<DailyRoomTypeAvailability>())
            .Where(availability => availability.Overbooked > 0)
            .Select(availability =>
                new DailyOverbookingDetected(
                    availability.RoomType,
                    availability.Date,
                    availability.Overbooked,
                    availability.OverbookedOverTheLimit
                )
            );

        foreach (var @event in events)
        {
            await eventBus.Publish(EventEnvelope.From(@event), token);
        }
    }
}
