using System.Threading;
using System.Threading.Tasks;
using Domain.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using Marten;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientsQueryHandler : IQueryHandler<GetClientView, ClientView>
    {
        private IDocumentSession _session;

        public ClientsQueryHandler(IDocumentSession session)
        {
            _session = session;
        }

        public Task<ClientView> Handle(GetClientView query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _session
                .Query<ClientView>()
                .SingleOrDefaultAsync(client => client.Id == query.Id, cancellationToken);
        }
    }
}