using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts;
using System.Collections.Generic;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class GetAccounts : IQuery<IEnumerable<AccountSummary>>
    {
    }
}
