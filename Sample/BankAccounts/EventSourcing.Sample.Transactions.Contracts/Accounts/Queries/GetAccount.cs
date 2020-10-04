using System;
using Core.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;

namespace EventSourcing.Sample.Tasks.Views.Account
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
