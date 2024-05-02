using Core.Commands;
using Core.Marten.Repository;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.SchedulingMeeting;

public record ScheduleMeeting(
    Guid MeetingId,
    DateRange Occurs
);

internal class HandleScheduleMeeting(IMartenRepository<Meeting> repository):
    ICommandHandler<ScheduleMeeting>
{
    public Task Handle(ScheduleMeeting command, CancellationToken ct)
    {
        var (meetingId, dateRange) = command;

        return repository.GetAndUpdate(
            meetingId,
            meeting => meeting.Schedule(dateRange),
            ct: ct
        );
    }
}
