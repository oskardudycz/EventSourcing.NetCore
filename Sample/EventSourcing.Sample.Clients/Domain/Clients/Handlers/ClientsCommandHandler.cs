using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.Events;
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

        private DbSet<Client> Clients;

        public ClientsCommandHandler(
            ClientsDbContext dbContext,
            IEventBus eventBus)
        {
            this.dbContext = dbContext;
            Clients = dbContext.Clients;
            this.eventBus = eventBus;
        }

        public async Task<Unit> Handle(CreateClient command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = new Client(
                command.Id.Value,
                command.Data.Name,
                command.Data.Email
            );

            await Clients.AddAsync(client);

            await SaveAndPublish(client, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(UpdateClient command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = await Clients.FindAsync(command.Id);

            client.Update(command.Data);

            dbContext.Update(client);

            await SaveAndPublish(client, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteClient command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = await Clients.FindAsync(command.Id);

            dbContext.Remove(client);

            await SaveAndPublish(client, cancellationToken);

            return Unit.Value;
        }

        private async Task SaveAndPublish(Client client, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(client.PendingEvents.ToArray());
        }
    }
}
