using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Core.Testing.Fixtures;

public class PostgresContainerFixture: IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:15.1-alpine")
        .WithReuse(true)
        .Build();

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync().ConfigureAwait(false);
        DataSource = new NpgsqlDataSourceBuilder(container.GetConnectionString()).Build();
    }

    public async Task DisposeAsync()
    {
        await DataSource.DisposeAsync().ConfigureAwait(false);
        // Other fixtures and test processes can still be using this reusable container.
    }
}
