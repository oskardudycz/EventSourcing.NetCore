using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace MeetingsManagement.Meetings.CreatingMeeting;

public record CreateMeeting(
    Guid Id,
    string Name
): ICommand;

internal class HandleCreateMeeting:
    ICommandHandler<CreateMeeting>
{
    private readonly IMartenRepository<Meeting> repository;
    private readonly IMartenAppendScope scope;

    public HandleCreateMeeting(
        IMartenRepository<Meeting> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CreateMeeting command, CancellationToken cancellationToken)
    {
        var (id, name) = command;

        await scope.Do((_, eventMetadata) =>
            repository.Add(
                Meeting.New(id, name),
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
