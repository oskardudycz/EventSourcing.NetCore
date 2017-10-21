﻿using EventSourcing.Sample.Clients.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Sample.Clients.Storage
{
    public class ClientsDbContext : DbContext
    {
        public ClientsDbContext(DbContextOptions<ClientsDbContext> options)
            : base(options)
        {
            #if DEBUG
            Database.Migrate();
            #endif
        }

        public DbSet<Client> Clients { get; set; }
    }
}
