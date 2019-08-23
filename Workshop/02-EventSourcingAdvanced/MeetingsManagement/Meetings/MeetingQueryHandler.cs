using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Core.Storage;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings
{
    internal class MeetingQueryHandler: IQueryHandler<GetMeeting, MeetingSummary>
    {
        private readonly IRepository<Meeting> repository;

        public MeetingQueryHandler(
            IRepository<Meeting> repository
        )
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<MeetingSummary> Handle(GetMeeting request, CancellationToken cancellationToken)
        {
            var meeting = await repository.Find(request.Id, cancellationToken);

            return new MeetingSummary { Id = meeting.Id, Name = meeting.Name };
        }
    }
}
