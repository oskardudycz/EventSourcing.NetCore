using Core.Aggregates;

namespace Tickets.Concerts;

public class Concert(string name, DateTime date): Aggregate
{
    public string Name { get; private set; } = name;

    public DateTime Date { get; private set; } = date;
}
