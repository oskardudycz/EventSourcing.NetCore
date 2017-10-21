using Domain.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Storage;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventSourcing.Sample.Clients.Domain.Clients.Handlers
{
    public class ClientsCommandHandler :
        IAsyncCommandHandler<CreateUser>,
        IAsyncCommandHandler<UpdateUser>,
        IAsyncCommandHandler<DeleteUser>
    {
        private readonly ClientsDbContext dbContext;

        private DbSet<Client> DbSet => dbContext.Users;

        public ClientsCommandHandler(ClientsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(CreateUser command)
        {
            await DbSet.AddAsync(new Client(
                Guid.NewGuid(), 
                command.Name,
                command.Email
            ));
            await dbContext.SaveChangesAsync();
        }

        public async Task Handle(UpdateUser command)
        {
            var user = await DbSet.FindAsync(command.Id);

            user.Update(command.Name, command.Email);

            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task Handle(DeleteUser command)
        {
            var user = await DbSet.FindAsync(command.Id);
            dbContext.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }
}
