using System;
using Core.Events;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.Events
{
    public class MeetingScheduled: IEvent
    {
        public Guid MeetingId { get; }
        public Range Occurs { get; }

        public MeetingScheduled(Guid meetingId, Range occurs)
        {
            MeetingId = meetingId;
            Occurs = occurs;
        }

        public static MeetingScheduled Create(Guid meetingId, Range occurs)
        {
            if (meetingId == default(Guid))
                throw new ArgumentException($"{nameof(meetingId)} needs to be defined.");

            if (occurs == default(Range))
                throw new ArgumentException($"{nameof(occurs)} needs to be defined.");

            return new MeetingScheduled(meetingId, occurs);
        }
    }
}
