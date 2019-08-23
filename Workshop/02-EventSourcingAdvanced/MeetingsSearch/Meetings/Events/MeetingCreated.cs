using System;
using Core.Events;

namespace MeetingsSearch.Meetings.Events
{
    internal class MeetingCreated: IEvent
    {
        public Guid Id { get; }
        public string Name { get; }

        public MeetingCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
