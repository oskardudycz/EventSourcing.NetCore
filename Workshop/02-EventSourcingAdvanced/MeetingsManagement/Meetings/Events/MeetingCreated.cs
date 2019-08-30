using System;
using Core.Events;

namespace MeetingsManagement.Meetings.Events
{
    public class MeetingCreated: IExternalEvent
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
