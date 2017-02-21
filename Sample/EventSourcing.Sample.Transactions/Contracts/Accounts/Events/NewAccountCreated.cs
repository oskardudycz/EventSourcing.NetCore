using Domain.Events;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Accounts.Events
{
    public class NewAccountCreated : IEvent
    {
        public Guid AccountId { get; }

        public Guid ClientId { get; }

        public NewAccountCreated(Guid accountId, Guid clientId)
        {
            AccountId = accountId;
            ClientId = clientId;
        }
    }
}
