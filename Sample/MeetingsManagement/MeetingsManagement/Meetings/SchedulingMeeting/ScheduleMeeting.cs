using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.SchedulingMeeting;

public record ScheduleMeeting(
    Guid MeetingId,
    DateRange Occurs
): ICommand;

internal class HandleScheduleMeeting:
    ICommandHandler<ScheduleMeeting>
{
    private readonly IMartenRepository<Meeting> repository;
    private readonly IMartenAppendScope scope;

    public HandleScheduleMeeting(
        IMartenRepository<Meeting> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(ScheduleMeeting command, CancellationToken cancellationToken)
    {
        var (meetingId, dateRange) = command;

        await scope.Do((expectedVersion, traceMetadata) =>
            repository.GetAndUpdate(
                meetingId,
                meeting => meeting.Schedule(dateRange),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
