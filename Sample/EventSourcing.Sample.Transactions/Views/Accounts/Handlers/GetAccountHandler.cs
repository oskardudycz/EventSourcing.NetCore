using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary;
using Marten;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Sample.Tasks.Views.Account;

namespace EventSourcing.Sample.Tasks.Views.Accounts.Handlers
{
    public class GetAccountHandler : IQueryHandler<GetAccount, AccountSummary>
    {
        private readonly IDocumentSession _session;

        public GetAccountHandler(IDocumentSession session)
        {
            _session = session;
        }

        public AccountSummary Handle(GetAccount message)
        {
            return _session
                .Query<AccountSummaryView>()
                .Select(a => new AccountSummary
                    {
                        AccountId = a.AccountId,
                        Balance = a.Balance,
                        ClientId = a.ClientId,
                        Number = a.Number,
                        TransactionsCount = a.TransactionsCount
                    })
                .FirstOrDefault(p => p.AccountId == message.AccountId);
        }
    }
}
