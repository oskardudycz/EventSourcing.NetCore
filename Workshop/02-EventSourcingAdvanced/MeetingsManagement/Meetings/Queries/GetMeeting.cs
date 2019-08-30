using System;
using Core.Queries;
using MeetingsManagement.Meetings.Views;

namespace MeetingsManagement.Meetings.Queries
{
    public class GetMeeting: IQuery<MeetingView>
    {
        public Guid Id { get; }

        public GetMeeting(Guid id)
        {
            Id = id;
        }
    }
}
