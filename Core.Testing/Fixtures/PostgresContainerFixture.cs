using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Core.Testing.Fixtures;

public class PostgresContainerFixture: IAsyncLifetime
{
    private readonly string[] connectionStringVariables;
    private PostgreSqlContainer? container;

    public PostgresContainerFixture(): this([])
    {
    }

    protected PostgresContainerFixture(params string[] connectionStringVariables) =>
        this.connectionStringVariables = connectionStringVariables;

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var port = await DockerCompose.PortAsync(DockerCompose.Project, "postgres", 5432).ConfigureAwait(false);
        if (port != null)
        {
            ConnectionString = $"Host=localhost;Port={port};Database=postgres;Username=postgres;Password=postgres";
        }
        else
        {
            container = new PostgreSqlBuilder("postgres:18.7-alpine").Build();
            await container.StartAsync().ConfigureAwait(false);
            ConnectionString = container.GetConnectionString();
        }

        DataSource = new NpgsqlDataSourceBuilder(ConnectionString).Build();

        foreach (var variable in connectionStringVariables)
            Environment.SetEnvironmentVariable(variable, ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        await DataSource.DisposeAsync().ConfigureAwait(false);

        if (container != null)
            await container.DisposeAsync().ConfigureAwait(false);
    }
}
