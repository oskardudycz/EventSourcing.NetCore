using Core.Commands;
using Core.Marten.Repository;

namespace MeetingsManagement.Meetings.CreatingMeeting;

public record CreateMeeting(
    Guid Id,
    string Name
);

internal class HandleCreateMeeting:
    ICommandHandler<CreateMeeting>
{
    private readonly IMartenRepository<Meeting> repository;

    public HandleCreateMeeting(IMartenRepository<Meeting> repository) =>
        this.repository = repository;

    public Task Handle(CreateMeeting command, CancellationToken cancellationToken)
    {
        var (id, name) = command;

        return repository.Add(
            Meeting.New(id, name),
            cancellationToken
        );
    }
}
