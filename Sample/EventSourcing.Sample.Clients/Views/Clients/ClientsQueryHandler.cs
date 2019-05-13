using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Domain.Clients;
using EventSourcing.Sample.Clients.Storage;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Sample.Clients.Views.Clients
{
    public class ClientsQueryHandler:
        IQueryHandler<GetClients, List<ClientListItem>>,
        IQueryHandler<GetClient, ClientItem>
    {
        private IQueryable<Client> Clients;

        public ClientsQueryHandler(ClientsDbContext dbContext)
        {
            Clients = dbContext.Clients;
        }

        public Task<List<ClientListItem>> Handle(GetClients query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Clients
                .Select(client => new ClientListItem
                {
                    Id = client.Id,
                    Name = client.Name
                })
                .ToListAsync(cancellationToken);
        }

        public Task<ClientItem> Handle(GetClient query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Clients
                .Select(client => new ClientItem
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email
                })
                .SingleOrDefaultAsync(client => client.Id == query.Id, cancellationToken);
        }
    }
}
