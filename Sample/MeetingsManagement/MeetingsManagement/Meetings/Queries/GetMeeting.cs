using System;
using Core.Queries;
using MeetingsManagement.Meetings.Views;

namespace MeetingsManagement.Meetings.Queries
{
    public record GetMeeting(
        Guid Id
    ): IQuery<MeetingView>;
}
