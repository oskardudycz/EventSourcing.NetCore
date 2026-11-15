using Testcontainers.PostgreSql;
using Xunit;

namespace IntroductionToEventSourcing.Tools;

public sealed class PostgresDatabase: IAsyncLifetime
{
    private PostgreSqlContainer? container;

    public async ValueTask InitializeAsync()
    {
        var port = await DockerCompose.PortAsync("postgres", 5432).ConfigureAwait(false);
        if (port != null)
        {
            TestDatabases.PostgresConnectionString =
                $"Host=localhost;Port={port};Database=postgres;Username=postgres;Password=postgres";
            return;
        }

        container = new PostgreSqlBuilder("postgres:18.7-alpine").Build();
        await container.StartAsync().ConfigureAwait(false);
        TestDatabases.PostgresConnectionString = container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        if (container != null)
            await container.DisposeAsync().ConfigureAwait(false);
    }
}
