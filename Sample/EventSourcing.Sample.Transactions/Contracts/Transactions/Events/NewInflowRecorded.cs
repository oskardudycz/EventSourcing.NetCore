using Domain.Events;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Transactions.Events
{
    public class NewInflowRecorded : IEvent
    {
        public Guid FromAccountId { get; }
        public Guid ToAccountId { get; }
        public Inflow Outflow { get; }

        public NewInflowRecorded(Guid fromAccountId, Guid toAccountId, Inflow outflow)
        {
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            Outflow = outflow;
        }
    }
}
