using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Core.ElasticSearch;

public class ElasticSearchConfig
{
    public string Url { get; set; } = default!;
    public string DefaultIndex { get; set; } = default!;
}

public static class ElasticSearchConfigExtensions
{
    private const string DefaultConfigKey = "ElasticSearch";
    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services, IConfiguration configuration, Action<ConnectionSettings>? config = null)
    {
        var elasticSearchConfig = configuration.GetSection(DefaultConfigKey).Get<ElasticSearchConfig>();

        var settings = new ConnectionSettings(new Uri(elasticSearchConfig.Url))
            .DefaultIndex(elasticSearchConfig.DefaultIndex);

        config?.Invoke(settings);

        var client = new ElasticClient(settings);

        return services.AddSingleton<IElasticClient>(client);
    }
}
