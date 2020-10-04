using System;
using Core.Commands;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.Commands
{
    public class ScheduleMeeting: ICommand
    {
        public Guid MeetingId { get; }
        public DateRange Occurs { get; }

        public ScheduleMeeting(Guid meetingId, DateRange occurs)
        {
            MeetingId = meetingId;
            Occurs = occurs;
        }
    }
}
