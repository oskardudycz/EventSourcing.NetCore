using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Core.Testing.Fixtures;

public class PostgresContainerFixture: IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithReuse(true)
        .Build();

    public NpgsqlDataSource DataSource { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await container.StartAsync().ConfigureAwait(false);
        DataSource = new NpgsqlDataSourceBuilder(container.GetConnectionString()).Build();
    }

    public async Task DisposeAsync()
    {
        await DataSource.DisposeAsync().ConfigureAwait(false);
        await container.StopAsync().ConfigureAwait(false);
    }
}
