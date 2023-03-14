using Core.Aggregates;

namespace Tickets.Concerts;

public class Event : Aggregate
{
    public string Name { get; }

    public DateTime Date { get; }

    public Event(string name, DateTime date)
    {
        Name = name;
        Date = date;
    }
}
