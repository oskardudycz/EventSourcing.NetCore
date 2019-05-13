using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Tasks.Views.Account;
using EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary;
using Marten;

namespace EventSourcing.Sample.Tasks.Views.Accounts.Handlers
{
    public class GetAccountHandler: IQueryHandler<GetAccount, AccountSummary>
    {
        private readonly IDocumentSession _session;

        public GetAccountHandler(IDocumentSession session)
        {
            _session = session;
        }

        public Task<AccountSummary> Handle(GetAccount message, CancellationToken cancellationToken = default(CancellationToken))
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
                .FirstOrDefaultAsync(p => p.AccountId == message.AccountId, cancellationToken);
        }
    }
}
