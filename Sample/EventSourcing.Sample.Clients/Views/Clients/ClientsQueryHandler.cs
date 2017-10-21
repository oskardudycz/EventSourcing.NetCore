using Domain.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Domain.Clients;
using EventSourcing.Sample.Clients.Storage;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcing.Sample.Clients.Views.Clients
{
    public class ClientsQueryHandler :
        IAsyncQueryHandler<GetClients, List<ClientListItem>>,
        IAsyncQueryHandler<GetClient, ClientItem>
    {
        private IQueryable<Client> Clients;

        public ClientsQueryHandler(ClientsDbContext dbContext)
        {
            Clients = dbContext.Clients;
        }

        public Task<List<ClientListItem>> Handle(GetClients query)
        {
            return Clients
                .Select(client => new ClientListItem
                {
                    Id = client.Id,
                    Name = client.Name
                })
                .ToListAsync();
        }

        public Task<ClientItem> Handle(GetClient query)
        {
            return Clients
                .Select(client => new ClientItem
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email
                })
                .SingleOrDefaultAsync(client => client.Id == query.Id);
        }
    }
}
