using EventSourcing.Sample.Clients.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Sample.Clients.Storage
{
    public class ClientsDbContext : DbContext
    {
        public ClientsDbContext(DbContextOptions<ClientsDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
    }
}
