using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary;
using Marten;

namespace EventSourcing.Sample.Transactions.Views.Accounts.Handlers
{
    public class GetAccountsHandler: IQueryHandler<GetAccounts, IEnumerable<Contracts.Accounts.ValueObjects.AccountSummary>>
    {
        private readonly IDocumentSession session;

        public GetAccountsHandler(IDocumentSession session)
        {
            this.session = session;
        }

        public async Task<IEnumerable<Contracts.Accounts.ValueObjects.AccountSummary>> Handle(GetAccounts message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await session.Query<AccountSummaryView>()
                .Select(
                    a => new Contracts.Accounts.ValueObjects.AccountSummary
                    {
                        AccountId = a.AccountId,
                        Balance = a.Balance,
                        ClientId = a.ClientId,
                        Number = a.Number,
                        TransactionsCount = a.TransactionsCount
                    }
                ).Where(p => p.ClientId == message.ClientId)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
