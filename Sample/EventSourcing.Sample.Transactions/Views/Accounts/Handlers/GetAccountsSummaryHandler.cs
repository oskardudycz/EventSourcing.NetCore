using Domain.Queries;
using EventSourcing.Sample.Tasks.Views.Accounts;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;
using Marten;
using System.Linq;

namespace EventSourcing.Sample.Transactions.Views.Accounts.Handlers
{
    public class GetAccountsSummaryHandler : IQueryHandler<GetAccountsSummary, AccountsSummary>
    {
        private readonly IDocumentSession _session;

        public GetAccountsSummaryHandler(IDocumentSession session)
        {
            _session = session;
        }

        public AccountsSummary Handle(GetAccountsSummary message)
        {
            return _session.Query<AccountsSummaryView>()
                  .Select(
                      a => new AccountsSummary
                      {
                          TotalBalance = a.TotalBalance,
                          TotalCount = a.TotalCount,
                          TotalTransactionsCount = a.TotalTransactionsCount
                      }
                  )
                  .SingleOrDefault();
        }
    }
}
