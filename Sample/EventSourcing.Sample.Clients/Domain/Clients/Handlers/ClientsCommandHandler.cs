using Domain.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Storage;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;

namespace EventSourcing.Sample.Clients.Domain.Clients.Handlers
{
    public class ClientsCommandHandler :
        IAsyncCommandHandler<CreateClient>,
        IAsyncCommandHandler<UpdateClient>,
        IAsyncCommandHandler<DeleteClient>
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

        public async Task Handle(CreateClient command)
        {
            var id = command.Id ?? Guid.NewGuid();

            await Clients.AddAsync(new Client(
                id, 
                command.Data.Name,
                command.Data.Email
            ));

            await dbContext.SaveChangesAsync();

            await eventBus.Publish(new ClientCreated(id, command.Data));
        }

        public async Task Handle(UpdateClient command)
        {
            var client = await Clients.FindAsync(command.Id);

            client.Update(command.Data);

            dbContext.Update(client);

            await dbContext.SaveChangesAsync();

            await eventBus.Publish(new ClientUpdated(command.Id, command.Data));
        }

        public async Task Handle(DeleteClient command)
        {
            var client = await Clients.FindAsync(command.Id);

            dbContext.Remove(client);

            await dbContext.SaveChangesAsync();

            await eventBus.Publish(new ClientDeleted(command.Id));
        }
    }
}
