using System.Runtime.InteropServices;
using Testcontainers.KurrentDb;
using Xunit;

namespace IntroductionToEventSourcing.Tools;

public sealed class KurrentDBDatabase: IAsyncLifetime
{
    private KurrentDbContainer? container;

    public async ValueTask InitializeAsync()
    {
        var port = await DockerCompose.PortAsync("kurrentdb", 2113).ConfigureAwait(false);
        if (port != null)
        {
            TestDatabases.KurrentDBConnectionString = $"kurrentdb://localhost:{port}?tls=false";
            return;
        }

        container = new KurrentDbBuilder(Image).Build();
        await container.StartAsync().ConfigureAwait(false);
        TestDatabases.KurrentDBConnectionString = container.GetConnectionString();
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
