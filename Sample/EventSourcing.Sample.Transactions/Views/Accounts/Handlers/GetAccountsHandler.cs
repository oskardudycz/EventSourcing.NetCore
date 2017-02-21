using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts;
using Marten;
using Marten.Events;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing.Sample.Tasks.Views.Accounts.Handlers
{
    public class GetAccountsHandler : IQueryHandler<GetAccounts, IEnumerable<AccountSummary>>
    {
        private readonly IDocumentSession _session;
        private IEventStore _store => _session.Events;

        public GetAccountsHandler(IDocumentSession session)
        {
            _session = session;
        }

        public IEnumerable<AccountSummary> Handle(GetAccounts message)
        {
            var document = _session.Query<AccountsSummaryView>().FirstOrDefault();
            return new List<AccountSummary>();
        }
    }
}
