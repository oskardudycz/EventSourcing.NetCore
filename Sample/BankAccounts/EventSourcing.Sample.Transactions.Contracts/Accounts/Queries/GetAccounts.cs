using System;
using System.Collections.Generic;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Queries
{
    public class GetAccounts: IQuery<IEnumerable<AccountSummary>>
    {
        public Guid ClientId { get; private set; }

        public GetAccounts(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}
