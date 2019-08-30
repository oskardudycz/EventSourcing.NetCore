using System;
using Core.Events;

namespace MeetingsSearch.Meetings.Events
{
    internal class MeetingCreated: IEvent
    {
        public Guid MeetingId { get; }
        public string Name { get; }

        public MeetingCreated(Guid meetingId, string name)
        {
            MeetingId = meetingId;
            Name = name;
        }
    }
}
