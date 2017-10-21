using System;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using System.Collections.Generic;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class GetAccounts : IQuery<IEnumerable<AccountSummary>>
    {
        public Guid ClientId { get; private set; }
        public GetAccounts(Guid clientId)
        {
            ClientId = clientId;
        }

    }
}
