using Core.Commands;
using Core.Marten.Repository;

namespace MeetingsManagement.Meetings.CreatingMeeting;

public record CreateMeeting(
    Guid Id,
    string Name
);

internal class HandleCreateMeeting(IMartenRepository<Meeting> repository):
    ICommandHandler<CreateMeeting>
{
    public Task Handle(CreateMeeting command, CancellationToken ct)
    {
        var (id, name) = command;

        return repository.Add(
            Meeting.New(id, name),
            ct
        );
    }
}
