using System;
using Core.Events;

namespace MeetingsManagement.Meetings.CreatingMeeting
{
    public class MeetingCreated: IExternalEvent
    {
        public Guid MeetingId { get; }
        public string Name { get; }
        public DateTime Created { get; }

        public MeetingCreated(Guid meetingId, string name, DateTime created)
        {
            MeetingId = meetingId;
            Name = name;
            Created = created;
        }

        public static MeetingCreated Create(Guid meetingId, string name, DateTime created)
        {
            if (meetingId == default(Guid))
                throw new ArgumentException($"{nameof(meetingId)} needs to be defined.");

            if (created == default(DateTime))
                throw new ArgumentException($"{nameof(created)} needs to be defined.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{nameof(name)} can't be empty.");

            return new MeetingCreated(meetingId, name, created);
        }
    }
}
