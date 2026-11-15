using Microsoft.EntityFrameworkCore;
using Core.Testing.Fixtures;
using Xunit;

namespace Core.EntityFramework.Tests.Fixtures;

public class EfCorePostgresContainerFixture<TContext>(PostgresContainerFixture postgresContainerFixture): IAsyncLifetime
    where TContext : DbContext
{
    public TContext DbContext { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(postgresContainerFixture.DataSource);

        DbContext = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;

        await DbContext.Database.MigrateAsync().ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => DbContext.DisposeAsync();
}

