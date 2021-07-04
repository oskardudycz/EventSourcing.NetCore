using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;

namespace MeetingsManagement.Meetings.GettingMeeting
{
    public record GetMeeting(
        Guid Id
    ): IQuery<MeetingView>;


    internal class HandleGetMeeting: IQueryHandler<GetMeeting, MeetingView?>
    {
        private readonly IDocumentSession session;

        public HandleGetMeeting(
            IDocumentSession session
        )
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public Task<MeetingView?> Handle(GetMeeting request, CancellationToken cancellationToken)
        {
            return session.LoadAsync<MeetingView>(request.Id, cancellationToken);
        }
    }
}
