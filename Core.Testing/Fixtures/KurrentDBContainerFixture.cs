using System.Runtime.InteropServices;
using Testcontainers.KurrentDb;
using Xunit;

namespace Core.Testing.Fixtures;

public class KurrentDBContainerFixture: IAsyncLifetime
{
    private readonly string[] connectionStringVariables;
    private KurrentDbContainer? container;

    public KurrentDBContainerFixture(): this([])
    {
    }

    protected KurrentDBContainerFixture(params string[] connectionStringVariables) =>
        this.connectionStringVariables = connectionStringVariables;

    public string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var port = await DockerCompose.PortAsync(DockerCompose.Project, "kurrentdb", 2113).ConfigureAwait(false);
        if (port != null)
        {
            ConnectionString = $"kurrentdb://localhost:{port}?tls=false";
        }
        else
        {
            container = new KurrentDbBuilder(Image).Build();
            await container.StartAsync().ConfigureAwait(false);
            ConnectionString = container.GetConnectionString();
        }

        foreach (var variable in connectionStringVariables)
            Environment.SetEnvironmentVariable(variable, ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (container != null)
            await container.DisposeAsync().ConfigureAwait(false);
    }

    private static string Image =>
        RuntimeInformation.OSArchitecture == Architecture.Arm64
            ? "kurrentplatform/kurrentdb:26.2.3-experimental-arm64-10.0-noble"
            : "kurrentplatform/kurrentdb:26.2.3";
}
