using Core.Queries;
using Marten;

namespace MeetingsManagement.Meetings.GettingMeeting;

public record GetMeeting(
    Guid Id
);


internal class HandleGetMeeting(IQuerySession session): IQueryHandler<GetMeeting, MeetingView?>
{
    public Task<MeetingView?> Handle(GetMeeting request, CancellationToken cancellationToken) =>
        session.LoadAsync<MeetingView>(request.Id, cancellationToken);
}
