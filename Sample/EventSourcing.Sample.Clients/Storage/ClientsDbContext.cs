using EventSourcing.Sample.Clients.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Sample.Clients.Storage
{
    public class ClientsDbContext: DbContext
    {
        public ClientsDbContext(DbContextOptions<ClientsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().Ignore(c => c.PendingEvents);
        }

        public DbSet<Client> Clients { get; set; }
    }
}
