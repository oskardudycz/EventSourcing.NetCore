using Domain.Commands;
using EventSourcing.Sample.Clients.Contracts.Users.Commands;
using EventSourcing.Sample.Clients.Storage;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventSourcing.Sample.Clients.Domain.Users.Handlers
{
    public class UserCommandHandler :
        IAsyncCommandHandler<CreateUser>,
        IAsyncCommandHandler<UpdateUser>,
        IAsyncCommandHandler<DeleteUser>
    {
        private readonly ClientsDbContext dbContext;

        private DbSet<User> DbSet => dbContext.Users;

        public UserCommandHandler(ClientsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(CreateUser command)
        {
            await DbSet.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Email = command.Email
            });
            await dbContext.SaveChangesAsync();
        }

        public async Task Handle(UpdateUser command)
        {
            var user = await DbSet.FindAsync(command.Id);

            user.Name = command.Name;
            user.Email = command.Email;

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
