using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.OptimisticConcurrency;
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
    private readonly MartenOptimisticConcurrencyScope scope;

    public HandleCreateMeeting(
        IMartenRepository<Meeting> repository,
        MartenOptimisticConcurrencyScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CreateMeeting command, CancellationToken cancellationToken)
    {
        var (id, name) = command;

        await scope.Do(_ =>
            repository.Add(
                Meeting.New(id, name),
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
