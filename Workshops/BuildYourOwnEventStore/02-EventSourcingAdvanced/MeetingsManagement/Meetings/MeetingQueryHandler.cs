using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.Views;

namespace MeetingsManagement.Meetings
{
    internal class MeetingQueryHandler: IQueryHandler<GetMeeting, MeetingView>
    {
        private readonly IDocumentSession session;

        public MeetingQueryHandler(
            IDocumentSession session
        )
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public Task<MeetingView> Handle(GetMeeting request, CancellationToken cancellationToken)
        {
            return session.LoadAsync<MeetingView>(request.Id, cancellationToken);
        }
    }
}
