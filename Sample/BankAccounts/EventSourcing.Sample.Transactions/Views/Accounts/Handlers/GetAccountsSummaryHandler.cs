using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Views.Accounts.AllAccountsSummary;
using Marten;

namespace EventSourcing.Sample.Transactions.Views.Accounts.Handlers
{
    public class GetAccountsSummaryHandler: IQueryHandler<GetAccountsSummary, Contracts.Accounts.ValueObjects.AllAccountsSummary?>
    {
        private readonly IDocumentSession session;

        public GetAccountsSummaryHandler(IDocumentSession session)
        {
            this.session = session;
        }

        public Task<Contracts.Accounts.ValueObjects.AllAccountsSummary?> Handle(GetAccountsSummary message, CancellationToken cancellationToken = default)
        {
            return session.Query<AllAccountsSummaryView>()
                  .Select(
                      a => new Contracts.Accounts.ValueObjects.AllAccountsSummary
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
