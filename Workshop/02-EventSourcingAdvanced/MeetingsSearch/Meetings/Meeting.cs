using System;
using Core.Aggregates;

namespace MeetingsSearch.Meetings
{
    public class Meeting: Aggregate
    {
        public string Name { get; private set; }

        public Meeting()
        {
        }

        public Meeting(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
