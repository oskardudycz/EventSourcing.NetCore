using System;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Queries
{
    public record GetAccount (
        Guid AccountId
    ): IQuery<AccountSummary>;
}
