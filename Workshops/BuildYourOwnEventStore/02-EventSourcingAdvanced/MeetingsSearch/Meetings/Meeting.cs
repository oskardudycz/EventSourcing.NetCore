using System;
using Core.Aggregates;
using Newtonsoft.Json;

namespace MeetingsSearch.Meetings
{
    public class Meeting: Aggregate
    {
        public string Name { get; private set; } = default!;

        public Meeting() { }

        [JsonConstructor]
        public Meeting(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
