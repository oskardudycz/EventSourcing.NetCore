using Domain.Events;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Transactions.Events
{
    public class NewOutflowRecorded : IEvent
    {
        public Guid AccountId { get; }

        public Outflow Outflow { get; }

        public NewOutflowRecorded(Guid accountId, Outflow outflow)
        {
            AccountId = accountId;
            Outflow = outflow;
        }
    }
}
