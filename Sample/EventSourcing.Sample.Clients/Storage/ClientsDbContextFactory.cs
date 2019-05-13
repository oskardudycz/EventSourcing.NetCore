using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EventSourcing.Sample.Clients.Storage
{
    public class ClientsDbContextFactory: IDesignTimeDbContextFactory<ClientsDbContext>
    {
        public ClientsDbContextFactory()
        {
        }

        public ClientsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ClientsDbContext>();

            if (optionsBuilder.IsConfigured)
                return new ClientsDbContext(optionsBuilder.Options);

            //Called by parameterless ctor Usually Migrations
            var environmentName = Environment.GetEnvironmentVariable("EnvironmentName") ?? "Development";

            var connectionString =
                new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build()
                    .GetConnectionString("ClientsDatabase");

            optionsBuilder.UseNpgsql(connectionString);

            return new ClientsDbContext(optionsBuilder.Options);
        }
    }
}
