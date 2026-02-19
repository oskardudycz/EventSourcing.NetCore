using Core.Events;

namespace MeetingsManagement.Meetings.CreatingMeeting;

public record MeetingCreated(
    Guid MeetingId,
    string Name,
    DateTime Created
): IExternalEvent
{
    public static MeetingCreated Create(Guid meetingId, string name, DateTime created)
    {
        if (meetingId == Guid.Empty)
            throw new ArgumentException($"{nameof(meetingId)} needs to be defined.");

        if (created == default)
            throw new ArgumentException($"{nameof(created)} needs to be defined.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{nameof(name)} can't be empty.");

        return new MeetingCreated(meetingId, name, created);
    }
}
