using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.SchedulingMeeting;

public record MeetingScheduled(
    Guid MeetingId,
    DateRange Occurs
)
{
    public static MeetingScheduled Create(Guid meetingId, DateRange occurs)
    {
        if (meetingId == Guid.Empty)
            throw new ArgumentException($"{nameof(meetingId)} needs to be defined.");

        if (occurs == null)
            throw new ArgumentException($"{nameof(occurs)} needs to be defined.");

        return new MeetingScheduled(meetingId, occurs);
    }
}
