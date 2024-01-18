using Core.Aggregates;

namespace MeetingsSearch.Meetings;

public class Meeting: Aggregate
{
    public string Name { get; }

    public Meeting(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
