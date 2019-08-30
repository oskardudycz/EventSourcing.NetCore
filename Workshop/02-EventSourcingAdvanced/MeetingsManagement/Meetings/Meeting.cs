using System;
using Core.Aggregates;
using MeetingsManagement.Meetings.Events;

namespace MeetingsManagement.Meetings
{
    internal class Meeting: Aggregate
    {
        public string Name { get; private set; }

        public Meeting()
        {
        }

        private Meeting(Guid id, string name)
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"{nameof(id)} cannot be empty.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{nameof(name)} cannot be empty.");

            var @event = new MeetingCreated(id, name);

            Enqueue(@event);
            Apply(@event);
        }

        public void Apply(MeetingCreated @event)
        {
            Id = @event.MeetingId;
            Name = @event.Name;
        }

        public static Meeting Create(Guid id, string name)
        {
            return new Meeting(id, name);
        }
    }
}
