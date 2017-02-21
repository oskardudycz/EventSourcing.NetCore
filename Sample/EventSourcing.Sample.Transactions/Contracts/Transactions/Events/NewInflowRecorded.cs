using Domain.Events;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Transactions.Events
{
    public class NewInflowRecorded : IEvent
    {
        public Guid AccountId { get; }

        public Inflow Outflow { get; }

        public NewInflowRecorded(Guid accountId, Inflow outflow)
        {
            AccountId = accountId;
            Outflow = outflow;
        }
    }
}
