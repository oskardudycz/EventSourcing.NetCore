using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Core.EventStoreDB.Tests.Subscriptions.Checkpoints;

public class PostgresContainerFixture: IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithReuse(true)
        .Build();

    public NpgsqlDataSource DataSource { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        DataSource = new NpgsqlDataSourceBuilder(container.GetConnectionString()).Build();
    }

    public async Task DisposeAsync()
    {
        await DataSource.DisposeAsync();
        await container.StopAsync();
    }

}
