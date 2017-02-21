using Domain.Events;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Accounts.Events
{
    public class NewAccountCreated : IEvent
    {
        public Guid AccountId { get; set; }

        public Guid ClientId { get; set; }
    }
}
