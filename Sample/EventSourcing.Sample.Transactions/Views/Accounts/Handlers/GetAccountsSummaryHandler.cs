using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Views.Accounts;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;
using Marten;

namespace EventSourcing.Sample.Transactions.Views.Accounts.Handlers
{
    public class GetAccountsSummaryHandler : IQueryHandler<GetAccountsSummary, AllAccountsSummary>
    {
        private readonly IDocumentSession _session;

        public GetAccountsSummaryHandler(IDocumentSession session)
        {
            _session = session;
        }

        public Task<AllAccountsSummary> Handle(GetAccountsSummary message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _session.Query<AllAccountsSummaryView>()
                  .Select(
                      a => new AllAccountsSummary
                      {
                          TotalBalance = a.TotalBalance,
                          TotalCount = a.TotalCount,
                          TotalTransactionsCount = a.TotalTransactionsCount
                      }
                  )
                  .SingleOrDefaultAsync(cancellationToken);
        }
    }
}