using Testcontainers.Elasticsearch;
using Xunit;

namespace Core.Testing.Fixtures;

public class ElasticsearchContainerFixture: IAsyncLifetime
{
    private readonly string[] urlVariables;
    private ElasticsearchContainer? container;

    public ElasticsearchContainerFixture(): this([])
    {
    }

    protected ElasticsearchContainerFixture(params string[] urlVariables) =>
        this.urlVariables = urlVariables;

    public string Url { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var port = await DockerCompose.PortAsync(DockerCompose.Project, "elasticsearch", 9200).ConfigureAwait(false);
        if (port != null)
        {
            Url = new UriBuilder(Uri.UriSchemeHttp, "localhost", port.Value).ToString();
        }
        else
        {
            container = new ElasticsearchBuilder("docker.elastic.co/elasticsearch/elasticsearch:9.5.6")
                .WithEnvironment("xpack.security.enabled", "false")
                .WithEnvironment("xpack.security.http.ssl.enabled", "false")
                .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
                .Build();
            await container.StartAsync().ConfigureAwait(false);
            Url = new UriBuilder(Uri.UriSchemeHttp, container.Hostname,
                container.GetMappedPublicPort(ElasticsearchBuilder.ElasticsearchHttpsPort)).ToString();
        }

        foreach (var variable in urlVariables)
            Environment.SetEnvironmentVariable(variable, Url);
    }

    public async ValueTask DisposeAsync()
    {
        if (container != null)
            await container.DisposeAsync().ConfigureAwait(false);
    }
}
