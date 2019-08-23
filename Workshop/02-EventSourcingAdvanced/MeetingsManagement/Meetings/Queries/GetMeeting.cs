using System;
using Core.Queries;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.Queries
{
    public class GetMeeting: IQuery<MeetingSummary>
    {
        public Guid Id { get; }

        public GetMeeting(Guid id)
        {
            Id = id;
        }
    }
}
