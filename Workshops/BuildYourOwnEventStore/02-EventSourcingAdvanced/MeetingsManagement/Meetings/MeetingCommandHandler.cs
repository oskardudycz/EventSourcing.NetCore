using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;
using MeetingsManagement.Meetings.Commands;

namespace MeetingsManagement.Meetings
{
    internal class MeetingCommandHandler:
        ICommandHandler<CreateMeeting>,
        ICommandHandler<ScheduleMeeting>
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
            var meeting = Meeting.New(request.Id, request.Name);

            await repository.Add(meeting, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(ScheduleMeeting request, CancellationToken cancellationToken)
        {
            var meeting = await repository.Find(request.MeetingId, cancellationToken);

            meeting.Schedule(request.Occurs);

            await repository.Update(meeting, cancellationToken);

            return Unit.Value;
        }
    }
}
