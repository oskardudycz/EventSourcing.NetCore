using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using EventSourcing.Sample.Clients.Storage;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Sample.Clients.Domain.Clients.Handlers
{
    public class ClientsCommandHandler :
        ICommandHandler<CreateClient>,
        ICommandHandler<UpdateClient>,
        ICommandHandler<DeleteClient>
    {
        private readonly ClientsDbContext dbContext;
        private readonly IEventBus eventBus;

        private DbSet<Client> Clients;

        public ClientsCommandHandler(ClientsDbContext dbContext, IEventBus eventBus)
        {
            this.dbContext = dbContext;
            Clients = dbContext.Clients;
            this.eventBus = eventBus;
        }

        public async Task Handle(CreateClient command, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Clients.AddAsync(new Client(
                command.Id.Value,
                command.Data.Name,
                command.Data.Email
            ));

            await dbContext.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(new ClientCreated(command.Id.Value, command.Data));
        }

        public async Task Handle(UpdateClient command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = await Clients.FindAsync(command.Id);

            client.Update(command.Data);

            dbContext.Update(client);

            await dbContext.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(new ClientUpdated(command.Id, command.Data));
        }

        public async Task Handle(DeleteClient command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = await Clients.FindAsync(command.Id);

            dbContext.Remove(client);

            await dbContext.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(new ClientDeleted(command.Id));
        }
    }
}