using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using Marten;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientsQueryHandler: IQueryHandler<GetClientView, ClientView?>
    {
        private IDocumentSession session;

        public ClientsQueryHandler(IDocumentSession session)
        {
            this.session = session;
        }

        public Task<ClientView?> Handle(GetClientView query, CancellationToken cancellationToken = default)
        {
            return session
                .Query<ClientView>()
                .SingleOrDefaultAsync(client => client.Id == query.Id, cancellationToken);
        }
    }
}
