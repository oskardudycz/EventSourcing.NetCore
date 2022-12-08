using Core.Commands;
using Core.Marten.Repository;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.SchedulingMeeting;

public record ScheduleMeeting(
    Guid MeetingId,
    DateRange Occurs
);

internal class HandleScheduleMeeting:
    ICommandHandler<ScheduleMeeting>
{
    private readonly IMartenRepository<Meeting> repository;

    public HandleScheduleMeeting(IMartenRepository<Meeting> repository) =>
        this.repository = repository;

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
