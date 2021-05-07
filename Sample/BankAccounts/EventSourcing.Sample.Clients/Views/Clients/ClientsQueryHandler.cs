using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Domain.Clients;
using EventSourcing.Sample.Clients.Storage;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Sample.Clients.Views.Clients
{
    public class ClientsQueryHandler:
        IQueryHandler<GetClients, List<ClientListItem>>,
        IQueryHandler<GetClient, ClientItem?>
    {
        private IQueryable<Client> clients;

        public ClientsQueryHandler(ClientsDbContext dbContext)
        {
            clients = dbContext.Clients;
        }

        public Task<List<ClientListItem>> Handle(GetClients query,
            CancellationToken cancellationToken = default)
        {
            return clients
                .Select(client => new ClientListItem(client.Id, client.Name))
                .ToListAsync(cancellationToken);
        }

        public Task<ClientItem?> Handle(GetClient query,
            CancellationToken cancellationToken = default)
        {
            return clients
                .Select(client => new ClientItem(client.Id, client.Name, client.Email))
                .SingleOrDefaultAsync(client => client.Id == query.Id, cancellationToken)!;
        }
    }
}
