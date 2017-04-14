using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary;
using Marten;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing.Sample.Tasks.Views.Accounts.Handlers
{
    public class GetAccountsHandler : IQueryHandler<GetAccounts, IEnumerable<AccountSummary>>
    {
        private readonly IDocumentSession _session;

        public GetAccountsHandler(IDocumentSession session)
        {
            _session = session;
        }

        public IEnumerable<AccountSummary> Handle(GetAccounts message)
        {
            return _session.Query<AccountSummaryView>()
                .Select(
                    a => new AccountSummary
                    {
                        AccountId = a.AccountId,
                        Balance = a.Balance,
                        ClientId = a.ClientId,
                        Number = a.Number,
                        TransactionsCount = a.TransactionsCount
                    }
                )
                .ToList();
        }
    }
}
