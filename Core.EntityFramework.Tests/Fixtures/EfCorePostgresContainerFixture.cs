using Microsoft.EntityFrameworkCore;
using Core.Testing.Fixtures;
using Xunit;

namespace Core.EntityFramework.Tests.Fixtures;

public class EfCorePostgresContainerFixture<TContext>: IAsyncLifetime where TContext : DbContext
{
    private readonly PostgresContainerFixture postgresContainerFixture = new();

    public TContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await postgresContainerFixture.InitializeAsync().ConfigureAwait(false);

        var optionsBuilder = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(postgresContainerFixture.DataSource);

        DbContext = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;

        await DbContext.Database.MigrateAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync().ConfigureAwait(false);
        await postgresContainerFixture.DisposeAsync().ConfigureAwait(false);
    }
}

