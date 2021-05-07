using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Sample.Clients.Domain.Clients.Handlers
{
    public class ClientsCommandHandler:
        ICommandHandler<CreateClient>,
        ICommandHandler<UpdateClient>,
        ICommandHandler<DeleteClient>
    {
        private readonly ClientsDbContext dbContext;
        private readonly IEventBus eventBus;

        private DbSet<Client> clients;

        public ClientsCommandHandler(
            ClientsDbContext dbContext,
            IEventBus eventBus)
        {
            this.dbContext = dbContext;
            clients = dbContext.Clients;
            this.eventBus = eventBus;
        }

        public async Task<Unit> Handle(CreateClient command, CancellationToken cancellationToken = default)
        {
            var client = new Client(
                command.Id,
                command.Data.Name,
                command.Data.Email
            );

            await clients.AddAsync(client, cancellationToken);

            await SaveAndPublish(client, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(UpdateClient command, CancellationToken cancellationToken = default)
        {
            var client = await clients.FindAsync(command.Id);

            client.Update(command.Data);

            dbContext.Update(client);

            await SaveAndPublish(client, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteClient command, CancellationToken cancellationToken = default)
        {
            var client = await clients.FindAsync(command.Id);

            dbContext.Remove(client);

            await SaveAndPublish(client, cancellationToken);

            return Unit.Value;
        }

        private async Task SaveAndPublish(Client client, CancellationToken cancellationToken = default)
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(client.DequeueUncommittedEvents().ToArray());
        }
    }
}
