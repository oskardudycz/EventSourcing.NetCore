using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary;
using Marten;

namespace EventSourcing.Sample.Transactions.Views.Accounts.Handlers
{
    public class GetAccountHandler: IQueryHandler<GetAccount, Contracts.Accounts.ValueObjects.AccountSummary?>
    {
        private readonly IDocumentSession session;

        public GetAccountHandler(IDocumentSession session)
        {
            this.session = session;
        }

        public Task<Contracts.Accounts.ValueObjects.AccountSummary?> Handle(GetAccount message, CancellationToken cancellationToken = default)
        {
            return session
                .Query<AccountSummaryView>()
                .Select(a => new Contracts.Accounts.ValueObjects.AccountSummary
                {
                    AccountId = a.AccountId,
                    Balance = a.Balance,
                    ClientId = a.ClientId,
                    Number = a.Number,
                    TransactionsCount = a.TransactionsCount
                })
                .FirstOrDefaultAsync(p => p.AccountId == message.AccountId, cancellationToken);
        }
    }
}
