using System;
using Core.Events;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Events
{
    public class NewAccountCreated: IEvent
    {
        public Guid AccountId { get; set; }

        public Guid ClientId { get; set; }

        public string Number { get; set; }
    }
}
