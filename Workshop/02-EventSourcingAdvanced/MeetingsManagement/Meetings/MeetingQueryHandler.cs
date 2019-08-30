using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings
{
    internal class MeetingQueryHandler: IQueryHandler<GetMeeting, MeetingSummary>
    {
        private readonly IDocumentSession session;

        public MeetingQueryHandler(
            IDocumentSession session
        )
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<MeetingSummary> Handle(GetMeeting request, CancellationToken cancellationToken)
        {
            var meeting = await session.LoadAsync<Meeting>(request.Id, cancellationToken);

            return new MeetingSummary { Id = meeting.Id, Name = meeting.Name };
        }
    }
}
