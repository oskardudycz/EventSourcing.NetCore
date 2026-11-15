using Testcontainers.Kafka;
using Xunit;

namespace Core.Testing.Fixtures;

public class KafkaContainerFixture: IAsyncLifetime
{
    private KafkaContainer? container;

    public string BootstrapServers { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var port = await DockerCompose.PortAsync(DockerCompose.Project, "kafka", 9092).ConfigureAwait(false);
        if (port != null)
        {
            BootstrapServers = $"localhost:{port}";
            return;
        }

        container = new KafkaBuilder("confluentinc/cp-kafka:8.4.1").Build();
        await container.StartAsync().ConfigureAwait(false);
        BootstrapServers = container.GetBootstrapAddress();
    }

    public async ValueTask DisposeAsync()
    {
        if (container != null)
            await container.DisposeAsync().ConfigureAwait(false);
    }
}
