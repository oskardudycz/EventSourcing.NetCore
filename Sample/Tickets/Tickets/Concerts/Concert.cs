using System;
using Core.Aggregates;

namespace Tickets.Concerts
{
    internal class Concert : Aggregate
    {
        public string Name { get; private set; }

        public DateTime Date { get; private set; }
    }
}
