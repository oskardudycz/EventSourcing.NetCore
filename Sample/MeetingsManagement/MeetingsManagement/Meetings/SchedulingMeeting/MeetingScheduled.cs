using System;
using Core.Events;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.SchedulingMeeting
{
    public class MeetingScheduled: IEvent
    {
        public Guid MeetingId { get; }
        public DateRange Occurs { get; }

        public MeetingScheduled(Guid meetingId, DateRange occurs)
        {
            MeetingId = meetingId;
            Occurs = occurs;
        }

        public static MeetingScheduled Create(Guid meetingId, DateRange occurs)
        {
            if (meetingId == default(Guid))
                throw new ArgumentException($"{nameof(meetingId)} needs to be defined.");

            if (occurs == default(DateRange))
                throw new ArgumentException($"{nameof(occurs)} needs to be defined.");

            return new MeetingScheduled(meetingId, occurs);
        }
    }
}
