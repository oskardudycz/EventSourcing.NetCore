using System;
using Core.Commands;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.Commands
{
    public class ScheduleMeeting: ICommand
    {
        public Guid MeetingId { get; }
        public Range Occurs { get; }

        public ScheduleMeeting(Guid meetingId, Range occurs)
        {
            MeetingId = meetingId;
            Occurs = occurs;
        }
    }
}
