using Core.Configuration;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ElasticSearch;

public class ElasticSearchConfig
{
    public string Url { get; set; } = null!;
    public string DefaultIndex { get; set; } = null!;
}

public static class ElasticSearchConfigExtensions
{
    private const string DefaultConfigKey = "ElasticSearch";

    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services, IConfiguration configuration, Action<ElasticsearchClientSettings>? config = null)
    {
        var elasticSearchConfig = configuration.GetRequiredConfig<ElasticSearchConfig>(DefaultConfigKey);

        var settings = new ElasticsearchClientSettings(new Uri(elasticSearchConfig.Url))
            .DefaultIndex(elasticSearchConfig.DefaultIndex);

        config?.Invoke(settings);

        var client = new ElasticsearchClient(settings);

        return services.AddSingleton(client);
    }
}
