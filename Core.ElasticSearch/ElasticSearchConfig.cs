using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Core.ElasticSearch
{
    public static class ElasticSearchConfig
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration, Action<ConnectionSettings>? config = null)
        {
            var url = configuration["elasticsearch:url"];
            var defaultIndex = configuration["elasticsearch:index"];

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex);

            config?.Invoke(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }
    }
}
