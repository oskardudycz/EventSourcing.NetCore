using System;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Queries
{
    public class GetAccount: IQuery<AccountSummary>
    {
        public Guid AccountId { get; private set; }

        public GetAccount(Guid accountId)
        {
            AccountId = accountId;
        }
    }
}
