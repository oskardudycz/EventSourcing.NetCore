using Core.Aggregates;

namespace Tickets.Concerts;

public class Event : Aggregate
{
    public string Name { get; private set; }

    public DateTime Date { get; private set; }

    public Event(string name, DateTime date)
    {
        Name = name;
        Date = date;
    }
}
