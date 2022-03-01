using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace MeetingsManagement.Meetings.CreatingMeeting
{
    public class CreateMeeting: ICommand
    {
        public Guid Id { get; }
        public string Name { get; }

        public CreateMeeting(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    internal class HandleCreateMeeting:
        ICommandHandler<CreateMeeting>
    {
        private readonly IMartenRepository<Meeting> repository;

        public HandleCreateMeeting(
            IMartenRepository<Meeting> repository
        )
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Unit> Handle(CreateMeeting command, CancellationToken cancellationToken)
        {
            var meeting = Meeting.New(command.Id, command.Name);

            await repository.Add(meeting, cancellationToken);

            return Unit.Value;
        }
    }
}
