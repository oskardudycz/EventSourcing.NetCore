using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Storage;
using MediatR;
using MeetingsManagement.Meetings.Commands;

namespace MeetingsManagement.Meetings
{
    internal class MeetingCommandHandler: ICommandHandler<CreateMeeting>
    {
        private readonly IRepository<Meeting> repository;

        public MeetingCommandHandler(
            IRepository<Meeting> repository
        )
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Unit> Handle(CreateMeeting request, CancellationToken cancellationToken)
        {
            var meeting = Meeting.Create(request.Id, request.Name);

            await repository.Add(meeting, cancellationToken);

            return Unit.Value;
        }
    }
}
